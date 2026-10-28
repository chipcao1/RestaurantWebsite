using System;
using System.Data;
using System.Linq;
using RestaurantWebsite.Models;
using RestaurantWebsite.Infrastructure;
namespace RestaurantWebsite.Services
{
    public sealed class ReservationService
    {
        private readonly AppDbContext db;
        public ReservationService(AppDbContext context) { db = context; }
        public bool HasConflict(int tableId, DateTime start, DateTime end, int? excludeReservationId = null)
        {
            return db.Reservations.Any(r => r.TableId == tableId && r.Id != (excludeReservationId ?? 0) && r.Status != "Cancelled" && r.Status != "Completed" && r.ReservationDate < end && System.Data.Entity.DbFunctions.AddHours(r.ReservationDate, 2) > start) ||
                db.Orders.Any(o => o.TableId == tableId && o.Status != "Cancelled" && o.Status != "Delivered" &&
                    (!excludeReservationId.HasValue || o.ReservationId != excludeReservationId) &&
                    (o.ServiceStart ?? o.CreatedAt) < end &&
                    ((!o.ServiceEnd.HasValue && System.Data.Entity.DbFunctions.AddHours(o.CreatedAt, 2) > start) || o.ServiceEnd > start || o.ServiceEnd <= RestaurantClock.Now));
        }
        private Reservation Get(int id, int actorId, string role)
        {
            var r = db.Reservations.SingleOrDefault(x => x.Id == id);
            if (r == null || (role == "Customer" && r.UserId != actorId) || (role == "Staff" && r.AssignedStaffId.HasValue && r.AssignedStaffId != actorId)) throw new InvalidOperationException("Không tìm thấy đặt bàn hoặc bạn không được phân công xử lý.");
            return r;
        }
        public void ChangeStatus(int id, string next, int actorId, string role)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var r = Get(id, actorId, role);
                bool open = db.Orders.Any(o => o.ReservationId == id && o.Status != "Cancelled" && o.Status != "Delivered");
                if (!WorkflowRules.ReservationTransition(r.Status, next, role, r.ReservationDate, RestaurantClock.Now, open)) throw new InvalidOperationException("Không thể chuyển trạng thái đặt bàn vào lúc này.");
                var table = db.RestaurantTables.Find(r.TableId);
                if (next == "Confirmed" && (table == null || !table.IsActive || table.Status != "Available" || table.Capacity < r.PartySize || HasConflict(r.TableId, r.ReservationDate, r.ReservationDate.AddHours(2), r.Id))) throw new InvalidOperationException("Bàn không phù hợp hoặc khung giờ đã có người đặt.");
                if (role == "Staff" && !r.AssignedStaffId.HasValue) r.AssignedStaffId = actorId;
                r.Status = next; db.SaveChanges(); tx.Commit();
            }
        }
        public void Assign(int id, int staffId)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var r = db.Reservations.SingleOrDefault(x => x.Id == id);
                if (r == null || (r.Status != "Pending" && r.Status != "Confirmed")) throw new InvalidOperationException("Đặt bàn đã kết thúc hoặc không tồn tại.");
                if (!db.Users.Any(u => u.Id == staffId && u.Role == "Staff" && u.IsActive)) throw new InvalidOperationException("Nhân viên không hợp lệ.");
                r.AssignedStaffId = staffId; db.SaveChanges(); tx.Commit();
            }
        }
        public void ChangeTable(int id, int tableId, int actorId, string role)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var r = Get(id, actorId, role); var table = db.RestaurantTables.Find(tableId);
                if (!WorkflowRules.IsOperator(role) || (r.Status != "Pending" && r.Status != "Confirmed") || db.Orders.Any(o => o.ReservationId == id && o.Status != "Cancelled" && o.Status != "Delivered")) throw new InvalidOperationException("Không thể đổi bàn khi đã phục vụ hoặc có đơn đang xử lý.");
                if (table == null || !table.IsActive || table.Status != "Available" || table.Capacity < r.PartySize || HasConflict(tableId, r.ReservationDate, r.ReservationDate.AddHours(2), id)) throw new InvalidOperationException("Bàn không đủ chỗ hoặc đã trùng lịch.");
                r.TableId = tableId; db.SaveChanges(); tx.Commit();
            }
        }
        public int Book(int userId, int tableId, DateTime start, int partySize, string note)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var table = db.RestaurantTables.Find(tableId);
                if (start <= RestaurantClock.Now || start > RestaurantClock.Now.AddMonths(3) || partySize < 1 || partySize > 50 || (note != null && note.Length > 500)) throw new InvalidOperationException("Vui lòng chọn giờ trong ba tháng tới, số khách hợp lệ và ghi chú tối đa 500 ký tự.");
                if (table == null || !table.IsActive || table.Status != "Available" || table.Capacity < partySize || HasConflict(tableId, start, start.AddHours(2))) throw new InvalidOperationException("Bàn không đủ chỗ hoặc đã có lịch phục vụ trong khung giờ này.");
                var r = new Reservation { UserId = userId, TableId = tableId, PartySize = partySize, ReservationDate = start, SpecialRequest = note, Status = "Pending", CreatedAt = RestaurantClock.Now };
                db.Reservations.Add(r); db.SaveChanges(); tx.Commit(); return r.Id;
            }
        }
    }
}
