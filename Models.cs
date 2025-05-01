using System;
using System.Collections.Generic;

namespace ScheduleAnalyzer
{
    public class ClassNameInfo
    {
        public string SubjectName { get; set; } = "";
        public List<string> CourseCodes { get; set; } = new List<string>();
    }

    public class ClassSession
    {
        /// <summary>
        /// Tên lớp học phần. Ví dụ: Giáo dục thể chất 5-2-24  (H32-CTL01.01)
        /// </summary>
        public string ClassName { get; set; } = "";

        /// <summary>
        /// Tên môn học (subject). Ví dụ: "Giáo dục thể chất 5"
        /// </summary>
        public string SubjectName { get; set; } = "";

        /// <summary>
        /// Danh sách các khóa học tham gia môn học. Ví dụ: [ "H32", "CTL01" ]
        /// </summary>
        public List<string> CourseCodes { get; set; } = new List<string>();

        /// <summary>
        /// Danh sách ca học của lớp học phần
        /// </summary>
        public List<Lesson> Lessons { get; set; } = new List<Lesson>();
    }

    public class Lesson
    {
        /// <summary>
        /// Thứ trong tuần của tiết học: 1 đến 6 (Thứ Hai đến Thứ Bảy) và CN được biểu diễn là 0.
        /// </summary>
        public int DayOfWeek { get; set; }

        /// <summary>
        /// Số thứ tự tiết bắt đầu ca học (ví dụ: 1 trong "1->3")
        /// </summary>
        public int StartPeriod { get; set; }

        /// <summary>
        /// Số thứ tự tiết kết thúc ca học (ví dụ: 3 trong "1->3")
        /// </summary>
        public int EndPeriod { get; set; }

        /// <summary>
        /// Ký hiệu phòng học (cột J trong file Excel)
        /// </summary>
        public string Room { get; set; } = "";

        /// <summary>
        /// Ngày bắt đầu áp dụng tiết học này
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc áp dụng tiết học này
        /// </summary>
        public DateTime EndDate { get; set; }
    }

    public class SubjectEndDate
    {
        public string SubjectName { get; set; } = "";
        public DateTime EndDate { get; set; }
    }

    public class FreeTimeSlot
    {
        public DateTime Date { get; set; }


        /// <summary>
        /// Nhóm tiết học: 1 -> (tiết 1-3), 2 -> (tiết 4-6), 3 -> (tiết 7-9), 4 -> (tiết 10-12), 5 -> (tiết 13-16)
        /// </summary>
        public int PeriodGroup { get; set; }


        /// <summary>
        /// Với mỗi khóa học, true nếu rảnh, false nếu bận
        /// </summary>
        public Dictionary<string, bool> CourseFree { get; set; } = new Dictionary<string, bool>();


        /// <summary>
        /// Với mỗi phòng học, true nếu rảnh, false nếu bận
        /// </summary>
        public Dictionary<string, bool> RoomFree { get; set; } = new Dictionary<string, bool>();
    }

    public class ExamSession
    {
        public string Subject { get; set; }
        public List<string> Courses { get; set; }
        public DateTime Date { get; set; }
        public int Period { get; set; }
        public string Room { get; set; }
    }

}
