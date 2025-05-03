using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScheduleAnalyzer
{
    public class TimetableReader
    {
        public static List<ClassSession> ReadSchedules(string inputFilePaths)
        {
            var result = new List<ClassSession>();

            // Tách các đường dẫn từ chuỗi input (mỗi đường dẫn nằm trong dấu ngoặc kép)
            var paths = Regex.Matches(inputFilePaths, "\"([^\"]+)\"")
                             .Cast<Match>()
                             .Select(m => m.Groups[1].Value)
                             .ToList();

            foreach (var path in paths)
            {
                var sessions = ReadSchedule(path);
                result.AddRange(sessions);
            }

            return result;
        }
        private static List<ClassSession> ReadSchedule(string filePath)
        {
            var sessions = new List<ClassSession>();
            IWorkbook workbook;

            // Phân biệt định dạng file theo phần mở rộng
            if (filePath.EndsWith(".xlsx"))
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                workbook = new XSSFWorkbook(fs);
                fs.Close();
            }
            else if (filePath.EndsWith(".xls"))
            {
                workbook = WorkbookFactory.Create(filePath);
            }
            else
            {
                throw new NotSupportedException("Định dạng tập tin không được hỗ trợ: " + filePath);
            }

            for (int s = 0; s < workbook.NumberOfSheets; s++)
            {
                ISheet sheet = workbook.GetSheetAt(s);
                if (sheet == null) continue;
                int headerRowIndex = FindHeaderRow(sheet);
                if (headerRowIndex == -1)
                    throw new Exception("Không tìm thấy dòng tiêu đề: " + sheet.SheetName);
                int rowIndex = headerRowIndex + 1;
                while (rowIndex <= sheet.LastRowNum)
                {
                    IRow row = sheet.GetRow(rowIndex);
                    if (row == null || IsRowEmpty(row)) break;

                    // Lấy tên lớp học phần từ cột E (index 4)
                    string className = GetMergedCellValue(sheet, rowIndex, 4);
                    if (string.IsNullOrWhiteSpace(className))
                    {
                        rowIndex++;
                        continue;
                    }

                    // Trích xuất tên môn học và danh sách khóa học
                    var classNameInfo = ParseClassName(className);
                    if (classNameInfo == null)
                    {
                        throw new Exception("Invalid class name: " + className);

                    }

                    // Tìm số dòng được gộp cho lớp học phần (cột E)
                    int mergedRowCount = GetMergedRowCount(sheet, rowIndex, 4);

                    List<Lesson> lessons = new List<Lesson>();
                    int count = 0;
                    do
                    {
                        // Đọc ngày bắt đầu và kết thúc cho giai đoạn này
                        string startDateStr = GetMergedCellValue(sheet, rowIndex + count, 10); // cột K
                        string endDateStr = GetMergedCellValue(sheet, rowIndex + count, 11);   // cột L
                        DateTime.TryParseExact(startDateStr, "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate);
                        DateTime.TryParseExact(endDateStr, "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate);

                        int subMergedRowCount = GetMergedRowCount(sheet, rowIndex + count, 10); // số dòng cho giai đoạn này

                        for (int i = 0; i < subMergedRowCount; i++)
                        {
                            IRow r = sheet.GetRow(rowIndex + count + i);
                            if (r == null) continue;

                            string dayStr = GetCellString(r, 7);      // Cột H: thứ trong tuần
                            string sessionStr = GetCellString(r, 8);  // Cột I: ca học
                            string room = GetCellString(r, 9);        // Cột J: phòng
                            Lesson lesson = new Lesson
                            {
                                DayOfWeek = ParseDayOfWeek(dayStr),
                                Room = room,
                                StartDate = startDate,
                                EndDate = endDate
                            };
                            (lesson.StartPeriod, lesson.EndPeriod) = ParseSession(sessionStr);
                            lessons.Add(lesson);
                        }

                        count += subMergedRowCount;

                    } while (count < mergedRowCount);

                    sessions.Add(new ClassSession
                    {
                        ClassName = className,
                        SubjectName = classNameInfo.SubjectName,
                        CourseCodes = classNameInfo.CourseCodes,
                        SubjectWithCourses = classNameInfo.SubjectName + "-" + String.Join("-", classNameInfo.CourseCodes),
                        Lessons = lessons
                    });
                    rowIndex += mergedRowCount;
                }
            }
            return sessions;
        }

        private static int FindHeaderRow(ISheet sheet)
        {
            // Dò tìm dòng tiêu đề có ô A = "TT" và ô D = "LL"
            for (int i = 0; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                if (GetCellString(row, 0) == "TT" && GetCellString(row, 3) == "LL")
                    return i;
            }
            return -1;
        }

        private static bool IsRowEmpty(IRow row)
        {
            foreach (var cell in row.Cells)
            {
                if (!string.IsNullOrEmpty(cell.ToString()))
                    return false;
            }
            return true;
        }

        private static string GetCellString(IRow row, int colIdx)
        {
            return row?.GetCell(colIdx)?.ToString().Trim() ?? "";
        }

        private static string GetMergedCellValue(ISheet sheet, int rowIdx, int colIdx)
        {
            // Kiểm tra các vùng ô gộp
            foreach (var region in sheet.MergedRegions)
            {
                if (region.FirstRow <= rowIdx && rowIdx <= region.LastRow && region.FirstColumn <= colIdx && colIdx <= region.LastColumn)
                {
                    IRow row = sheet.GetRow(region.FirstRow);
                    return GetCellString(row, region.FirstColumn);
                }
            }
            IRow r = sheet.GetRow(rowIdx);
            return GetCellString(r, colIdx);
        }

        private static int GetMergedRowCount(ISheet sheet, int startRow, int colIdx)
        {
            foreach (var region in sheet.MergedRegions)
            {
                if (region.FirstRow == startRow && region.FirstColumn == colIdx)
                    return region.LastRow - region.FirstRow + 1;
            }
            return 1;
        }

        private static ClassNameInfo? ParseClassName(string className)
        {
            var info = new ClassNameInfo();

            // Pattern để tách phần Subject và phần COURSE_SET trong ngoặc
            string pattern = @"^(.*?)-[0-9]-[0-9]{2}\s*\((.*?)\)$";
            var match = Regex.Match(className, pattern);

            //Không khớp mẫu
            if (!match.Success)
                return null;

            // Lấy phần Subject
            info.SubjectName = match.Groups[1].Value.Trim();

            // Lấy phần COURSE_SET và xử lý các COURSE
            string courseSet = match.Groups[2].Value;

            // Pattern để tìm các COURSE trong COURSE_SET
            // Mỗi COURSE gồm: 1+ chữ cái in hoa + 2 chữ số
            string coursePattern = @"([A-Z]+[0-9]{2})";
            var courseMatches = Regex.Matches(courseSet, coursePattern);

            //Nếu không khớp mẫu
            if (courseMatches.Count <= 0)
                return null;

            foreach (Match courseMatch in courseMatches)
            {
                info.CourseCodes.Add(courseMatch.Value);
            }

            //Return
            return info;
        }

        // Chuyển chuỗi thứ trong tuần thành số: "CN" trả về 0, số còn lại theo thứ
        private static int ParseDayOfWeek(string dayStr)
        {
            if (dayStr == "CN") return 0;
            if (int.TryParse(dayStr, out int day))
                return day-1;
            return -1;
        }

        // Phân tích ca học: định dạng "1->3" → trả về (1, 3)
        private static (int, int) ParseSession(string sessionStr)
        {
            string[] parts = sessionStr.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                int start = int.Parse(parts[0]);
                int end = int.Parse(parts[1]);
                return (start, end);
            }
            return (0, 0);
        }
    }
}
