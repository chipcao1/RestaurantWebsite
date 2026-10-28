using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using RestaurantWebsite.Models;
using RestaurantWebsite.Services;
using RestaurantWebsite.Infrastructure;
using RestaurantWebsite.Filters;
using PdfSharp.Pdf.IO;
class RequestStub : HttpRequestBase
{
    readonly string path; public RequestStub(string value){path=value;}
    public override string AppRelativeCurrentExecutionFilePath=>"~/"+path.TrimStart('/');
    public override string PathInfo=>"";public override string HttpMethod=>"GET";public override string RawUrl=>"/"+path;
}
class SessionStub:HttpSessionStateBase
{
    readonly Dictionary<string,object> data=new Dictionary<string,object>();
    public override object this[string name]{get=>data.ContainsKey(name)?data[name]:null;set=>data[name]=value;}
    public override void Clear(){data.Clear();}
}
class ContextStub:HttpContextBase
{
    readonly RequestStub request;readonly SessionStub session=new SessionStub();
    public ContextStub(string path){request=new RequestStub(path);}
    public override HttpRequestBase Request=>request;public override HttpSessionStateBase Session=>session;
}
class AuthorizeProbe:RoleAuthorizeAttribute
{
    public AuthorizeProbe(params string[] roles):base(roles){}
    public bool Allows(HttpContextBase context)=>base.AuthorizeCore(context);
}
class Program
{
    static int passed;
    static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);passed++;}
    static void Denied(Action action,string name){bool denied=false;try{action();}catch(InvalidOperationException){denied=true;}Check(denied,name);}
    static void Routes()
    {
        var routes=new RouteCollection();
        new RestaurantWebsite.Areas.Admin.AdminAreaRegistration().RegisterArea(new AreaRegistrationContext("Admin",routes));
        new RestaurantWebsite.Areas.Customer.CustomerAreaRegistration().RegisterArea(new AreaRegistrationContext("Customer",routes));
        new RestaurantWebsite.Areas.Staff.StaffAreaRegistration().RegisterArea(new AreaRegistrationContext("Staff",routes));
        RestaurantWebsite.RouteConfig.RegisterRoutes(routes);
        foreach(var spec in new[]{new[]{"admin","Admin","Dashboard","Index"},new[]{"staff/Orders","Staff","Staff","Orders"},new[]{"customer/orders","Customer","Account","MyOrders"},new[]{"Menu/Index","Customer","Menu","Index"},new[]{"Account/Login","","Account","Login"},new[]{"Account/Profile","Customer","Account","Profile"},new[]{"staff/catalog/Dishes/Edit/1","Staff","Dishes","Edit"}})
        {var route=routes.GetRouteData(new ContextStub(spec[0]));Check(route!=null&&Convert.ToString(route.DataTokens["area"])==spec[1]&&Convert.ToString(route.Values["controller"])==spec[2]&&Convert.ToString(route.Values["action"])==spec[3],"route "+spec[0]);}
    }
    static void Main()
    {
        if(Environment.OSVersion.Platform!=PlatformID.Win32NT)throw new PlatformNotSupportedException("Run these integration checks on Windows with LocalDB.");
        Routes();
        string database="RestaurantWebsite_Yamy_Test_"+Guid.NewGuid().ToString("N");
        string connection=new SqlConnectionStringBuilder{DataSource=@"(localdb)\MSSQLLocalDB",InitialCatalog=database,IntegratedSecurity=true,MultipleActiveResultSets=true}.ConnectionString;
        Environment.SetEnvironmentVariable("RESTAURANT_CONNECTION_STRING",connection);
        Environment.SetEnvironmentVariable("RESTAURANT_SEED_DEMO","false");
        try
        {
            var config=(DbMigrationsConfiguration<AppDbContext>)Activator.CreateInstance(typeof(AppDbContext).Assembly.GetType("RestaurantWebsite.Migrations.Configuration",true),true);
            config.TargetDatabase=new DbConnectionInfo(connection,"System.Data.SqlClient");
            var migrator=new DbMigrator(config);
            Check(migrator.GetLocalMigrations().SequenceEqual(new[]{"202609281435011_InitialCreate"}),"only main's InitialCreate migration exists");
            migrator.Update();
            Database.SetInitializer<AppDbContext>(null);
            int customer,staff,admin,dishId,tableId,promoId;
            using(var db=new AppDbContext())
            {
                Check(db.Database.CompatibleWithModel(true),"current EF model is compatible with main's database");
                Check(db.Database.SqlQuery<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME IN ('Comboes','Combos')").Single()=="Comboes","combo table retains main's original name");
                var users=new[]{new User{FullName="Khách thử",Email="customer@example.test",Role="Customer",PasswordHash="test-only",IsActive=true},new User{FullName="Nhân viên thử",Email="staff@example.test",Role="Staff",PasswordHash="test-only",IsActive=true},new User{FullName="Quản trị thử",Email="admin@example.test",Role="Admin",PasswordHash="test-only",IsActive=true}};
                db.Users.AddRange(users);var category=new Category{Name="Món thử",IsActive=true};db.Categories.Add(category);var dish=new Dish{Name="Món kiểm thử",Category=category,Price=100000,Status="Available",PreparationTime=15};db.Dishes.Add(dish);var table=new RestaurantTable{TableNumber="T1",Capacity=4,Zone="Indoor",Status="Available",IsActive=true};db.RestaurantTables.Add(table);var promo=new Promotion{Code="LASTONE",Name="Một lượt",DiscountType="Percentage",DiscountValue=10,MinimumOrderAmount=0,UsageLimit=1,StartDate=RestaurantClock.Now.AddDays(-1),EndDate=RestaurantClock.Now.AddDays(1)};db.Promotions.Add(promo);db.SaveChanges();customer=users[0].Id;staff=users[1].Id;admin=users[2].Id;dishId=dish.Id;tableId=table.Id;promoId=promo.Id;
            }
            // Re-running Update-Database on the main schema must preserve user data.
            using(var db=new AppDbContext())
            {
                db.Categories.Add(new Category{Name="Appetizers",Description="Fresh appetizers & starters",IsActive=true});
                db.Combos.Add(new Combo{Name="Family Combo",Description="Complete combo for family.",Price=100000,IsActive=true});
                db.SaveChanges();
            }
            migrator.Update();
            using(var db=new AppDbContext())
            {
                Check(!migrator.GetPendingMigrations().Any(),"no additional migrations pending for main database");
                Check(db.Categories.Any(c=>c.Name=="Appetizers"&&c.Description=="Fresh appetizers & starters")&&db.Combos.Any(c=>c.Name=="Family Combo"&&c.Description=="Complete combo for family."),"Update-Database preserves existing catalog data");
            }
            var fake=new ContextStub("admin");fake.Session["UserId"]=customer;fake.Session["Role"]="Admin";Check(!new AuthorizeProbe("Admin").Allows(fake),"session cannot spoof Admin role");
            using(var db=new AppDbContext()){var u=db.Users.Find(customer);u.IsActive=false;db.SaveChanges();}Check(!new AuthorizeProbe("Customer").Allows(fake),"locked user loses access immediately");
            using(var db=new AppDbContext()){db.Users.Find(customer).IsActive=true;db.SaveChanges();}
            int orderId;
            using(var db=new AppDbContext())
            {
                var service=new CheckoutService(db);Denied(()=>service.Place(customer,new Dictionary<int,int>{{dishId,1}},new Dictionary<int,int>(),new Order{OrderType="Delivery",Phone="0901234567"},"Cash",null),"delivery requires address");
                orderId=service.Place(customer,new Dictionary<int,int>{{dishId,1}},new Dictionary<int,int>(),new Order{OrderType="Takeaway",Phone="0901234567"},"Cash",null);
                Check(db.Orders.Find(orderId).TotalAmount==100000,"checkout uses database price");
            }
            using(var db=new AppDbContext())
            {
                var workflow=new OrderWorkflowService(db);Denied(()=>workflow.ChangeStatus(orderId,"Delivered",staff,"Staff"),"cannot jump to completion");
                workflow.ChangeStatus(orderId,"Confirmed",staff,"Staff");workflow.ChangeStatus(orderId,"Preparing",staff,"Staff");workflow.ChangeStatus(orderId,"Ready",staff,"Staff");Denied(()=>workflow.ChangeStatus(orderId,"Delivered",staff,"Staff"),"unpaid order cannot finish");
                int payment=db.Payments.Single(p=>p.OrderId==orderId).Id;workflow.ChangePayment(payment,"Completed",staff,"TEST-PAID");Denied(()=>workflow.ChangePayment(payment,"Completed",staff,"TEST-PAID"),"duplicate collection rejected");workflow.ChangeStatus(orderId,"Delivered",staff,"Staff");
                new ReviewService(db).Submit(customer,orderId,dishId,5,"Món ngon");Denied(()=>new ReviewService(db).Submit(customer,orderId,dishId,4,"Lặp"),"duplicate review rejected");Check(!db.Reviews.Single().IsApproved,"review waits for moderation");
            }
            Race(()=>{using(var db=new AppDbContext())new CheckoutService(db).Place(customer,new Dictionary<int,int>{{dishId,1}},new Dictionary<int,int>(),new Order{OrderType="Dine-in",TableId=tableId,Phone="0901234567"},"Cash",null);},"two checkouts cannot claim one table");
            Race(()=>{using(var db=new AppDbContext())new CheckoutService(db).Place(customer,new Dictionary<int,int>{{dishId,1}},new Dictionary<int,int>(),new Order{OrderType="Takeaway",Phone="0901234567"},"Cash","LASTONE");},"last promotion use cannot be oversubscribed");
            using(var db=new AppDbContext()){Check(db.Promotions.Find(promoId).UsedCount==1,"promotion count is atomic");Check(db.Orders.Count(o=>o.TableId==tableId&&o.Status!="Cancelled")==1,"one table claim persisted");}
            byte[] pdf=PdfService.Create("BÁO CÁO TIẾNG VIỆT",Enumerable.Range(1,180).Select(i=>"Dòng "+i+": Nguyễn Văn An — Thanh toán, hoàn tiền, đặt bàn."));
            Check(Encoding.ASCII.GetString(pdf,0,4)=="%PDF","PDF header");Check(Encoding.ASCII.GetString(pdf).Contains("/ToUnicode"),"PDF includes Unicode glyph map");using(var stream=new MemoryStream(pdf))using(var document=PdfReader.Open(stream,PdfDocumentOpenMode.Import))Check(document.PageCount>1,"PDF paginates long reports");
            Console.WriteLine(passed+" Windows checks passed");
        }
        finally
        {
            SqlConnection.ClearAllPools();using(var db=new AppDbContext())db.Database.Delete();
            Environment.SetEnvironmentVariable("RESTAURANT_CONNECTION_STRING",null);
        }
    }
    static void Race(Action operation,string name)
    {
        int successes=0;var gate=new Barrier(2);var tasks=Enumerable.Range(0,2).Select(_=>Task.Run(()=>{gate.SignalAndWait();try{operation();Interlocked.Increment(ref successes);}catch(System.Data.DataException){}catch(InvalidOperationException){} } )).ToArray();Task.WaitAll(tasks);Check(successes==1,name);gate.Dispose();
    }
}
