using System;
using System.Data.Entity;
using System.Linq;
using RestaurantWebsite.Models;

namespace RestaurantWebsite.Security
{
    /// <summary>
    /// Cung cấp logic nghiệp vụ dùng chung để kiểm tra xung đột thời gian đặt bàn.
    /// Khung giờ tiêu chuẩn: 2 giờ kể từ thời điểm đặt (Start đến Start + 2h).
    /// </summary>
    public static class ReservationConflictValidator
    {
        public const int ReservationDurationHours = 2;

        /// <summary>
        /// Kiểm tra xem một bàn cụ thể có bị trùng lịch với bất kỳ đặt bàn nào chưa bị Cancelled hay không.
        /// Sử dụng công thức giao khoảng thời gian:
        /// (ExistingStart < RequestedEnd) && (ExistingEnd > RequestedStart).
        /// </summary>
        /// <param name="db">AppDbContext hiện hành</param>
        /// <param name="tableId">ID của bàn cần kiểm tra</param>
        /// <param name="reservationDate">Thời điểm bắt đầu đặt bàn</param>
        /// <param name="excludeReservationId">ID đặt bàn cần loại trừ (ví dụ khi update chính nó)</param>
        /// <returns>True nếu có xung đột trùng lịch, False nếu bàn còn trống</returns>
        public static bool HasTimeConflict(AppDbContext db, int tableId, DateTime reservationDate, int? excludeReservationId = null)
        {
            DateTime requestedStart = reservationDate;
            DateTime requestedEnd = requestedStart.AddHours(ReservationDurationHours);

            var query = db.Reservations.AsQueryable();

            if (excludeReservationId.HasValue && excludeReservationId.Value > 0)
            {
                query = query.Where(r => r.Id != excludeReservationId.Value);
            }

            return query.Any(r =>
                r.TableId == tableId &&
                r.Status != "Cancelled" &&
                r.ReservationDate < requestedEnd &&
                DbFunctions.AddHours(r.ReservationDate, ReservationDurationHours) > requestedStart);
        }
    }
}
