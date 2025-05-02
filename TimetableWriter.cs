using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ScheduleAnalyzer
{
    public class TimetableWriter
    {
        public static void WriteEndDates(IWorkbook workbook, List<SubjectEndDate> subjectEndDates)
        {
            // Sắp xếp theo ngày kết thúc (EndDate)
            var sortedList = subjectEndDates.OrderBy(s => s.EndDate).ToList();

            ISheet sheet = workbook.CreateSheet("Ngày kết thúc");

            // Tạo style định dạng ngày cho cột D
            ICellStyle dateStyle = workbook.CreateCellStyle();
            short dateFormat = workbook.CreateDataFormat().GetFormat("dd/MM/yyyy");
            dateStyle.DataFormat = dateFormat;

            // Ghi tiêu đề
            IRow header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("Tên môn học");
            header.CreateCell(1).SetCellValue("Ngày kết thúc");
            header.CreateCell(2).SetCellValue("Thứ");
            header.CreateCell(3).SetCellValue("End Date");

            // Ghi dữ liệu
            for (int i = 0; i < sortedList.Count; i++)
            {
                var data = sortedList[i];
                IRow row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(data.SubjectName);
                row.CreateCell(1).SetCellValue(data.EndDate.ToString("dd/MM/yyyy")); // dạng chuỗi
                row.CreateCell(2).SetCellValue(GetDayOfWeekName(data.EndDate.DayOfWeek)); // Thứ

                ICell cellDate = row.CreateCell(3); // cột D
                cellDate.SetCellValue(data.EndDate);
                cellDate.CellStyle = dateStyle;
            }

            // Tự động điều chỉnh độ rộng cột
            for (int col = 0; col <= 3; col++)
                sheet.AutoSizeColumn(col);
        }

        public static void WriteBusynesses(IWorkbook workbook, List<FreeTimeSlot> freeTimeSlots)
        {
            ISheet sheet = workbook.CreateSheet("Có giờ");
            IRow header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("Ngày");
            header.CreateCell(1).SetCellValue("Thứ");
            header.CreateCell(2).SetCellValue("Nhóm tiết");
            DateTime today = DateTime.Today;

            // Xác định cột bắt đầu cho thông tin khóa học và phòng học
            var sampleSlot = freeTimeSlots.FirstOrDefault();
            int colIndex = 3;
            if (sampleSlot != null)
            {
                foreach (var course in sampleSlot.CourseFree.Keys)
                {
                    header.CreateCell(colIndex++).SetCellValue(course);
                }
                foreach (var room in sampleSlot.RoomFree.Keys)
                {
                    header.CreateCell(colIndex++).SetCellValue(room);
                }
            }
            // Ghi các dòng dữ liệu cho mỗi ngày và nhóm tiết
            int rowIdx = 1;
            var groupsByDate = freeTimeSlots.GroupBy(s => s.Date).OrderBy(g => g.Key);
            foreach (var group in groupsByDate)
            {
                //Không hiển thị những ngày đã qua
                if (group.First().Date < DateTime.Today)
                    continue;

                int startRow = rowIdx;
                foreach (var slot in group.OrderBy(s => s.PeriodGroup))
                {

                    IRow row = sheet.CreateRow(rowIdx++);
                    if (slot.PeriodGroup == group.Min(s => s.PeriodGroup))
                    {
                        row.CreateCell(0).SetCellValue(slot.Date.ToString("dd/MM/yyyy"));
                        row.CreateCell(1).SetCellValue(GetDayOfWeekName(slot.Date.DayOfWeek));
                    }
                    row.CreateCell(2).SetCellValue(slot.PeriodGroup.ToString());
                    colIndex = 3;
                    foreach (var free in slot.CourseFree.Values)
                    {
                        row.CreateCell(colIndex++).SetCellValue(free ? "" : "X");
                    }

                    //Đánh dấu "X" nếu có lớp học
                    //foreach (var free in slot.RoomFree.Values)
                    //{
                    //    row.CreateCell(colIndex++).SetCellValue(free ? "" : "X");
                    //}


                    //Ghi tên lớp học phần thay vì đơn thuần đánh dấu "X"
                    foreach (var free in slot.RoomClass.Values)
                    {
                        row.CreateCell(colIndex++).SetCellValue(free);
                    }
                }
                // Gộp các ô ngày và thứ cho cùng 1 ngày (mỗi ngày được gộp 5 dòng)
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(startRow, rowIdx - 1, 0, 0));
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(startRow, rowIdx - 1, 1, 1));
            }
        }

        public static void WriteExamSchedule(IWorkbook workbook, List<ExamSession> examSessions)
        {
            ISheet sheet = workbook.CreateSheet("Lịch thi");

            // Tạo style định dạng ngày
            ICellStyle dateStyle = workbook.CreateCellStyle();
            short dateFormat = workbook.CreateDataFormat().GetFormat("dd/MM/yyyy");
            dateStyle.DataFormat = dateFormat;

            // Ghi tiêu đề
            IRow header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("Môn học");
            header.CreateCell(1).SetCellValue("Khóa học");
            header.CreateCell(2).SetCellValue("Ngày thi");
            header.CreateCell(3).SetCellValue("Nhóm tiết");
            header.CreateCell(4).SetCellValue("Địa điểm");

            for (int i = 0; i < examSessions.Count; i++)
            {
                var exam = examSessions[i];
                IRow row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(exam.Subject);
                row.CreateCell(1).SetCellValue(string.Join(", ", exam.Courses));

                // Ngày thi với định dạng ngày thực tế (để dùng sort/format)
                ICell dateCell = row.CreateCell(2);
                dateCell.SetCellValue(exam.Date);
                dateCell.CellStyle = dateStyle;

                row.CreateCell(3).SetCellValue(exam.Period.ToString());
                row.CreateCell(4).SetCellValue(exam.Room);
            }

            // Auto fit
            for (int col = 0; col <= 4; col++)
                sheet.AutoSizeColumn(col);
        }

        private static string GetDayOfWeekName(DayOfWeek dow)
        {
            return dow switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "CN",
                _ => ""
            };
        }
    }
}
