using System;
using System.Collections.Generic;
using System.Linq;

namespace ScheduleAnalyzer
{
    public class TimetableProcessor
    {
        // Tính ngày kết thúc của mỗi môn học (subject) dựa trên các lớp học phần có cùng tên môn học.
        public static List<SubjectEndDate> GetSubjectEndDates(List<ClassSession> sessions)
        {
            var result = new List<SubjectEndDate>();

            // Nhóm các lớp học phần theo tên môn học
            var groups = sessions.GroupBy(s => s.SubjectName);

            foreach (var group in groups)
            {
                DateTime latestDate = DateTime.MinValue;

                foreach (var session in group)
                {
                    foreach (var lesson in session.Lessons)
                    {
                        DateTime date = lesson.StartDate.Date;
                        while (date <= lesson.EndDate.Date)
                        {
                            if ((int)date.DayOfWeek == lesson.DayOfWeek)
                            {
                                if (date > latestDate)
                                    latestDate = date;
                            }

                            date = date.AddDays(1);
                        }
                    }
                }

                if (latestDate != DateTime.MinValue)
                {
                    result.Add(new SubjectEndDate
                    {
                        SubjectName = group.Key,
                        EndDate = latestDate
                    });
                }
            }

            return result;
        }

        // Tính thời gian rỗi của các khóa học và phòng học.
        // Mỗi ngày từ hôm nay đến ngày kết thúc sớm nhất trong các lớp học phần,
        // với mỗi nhóm tiết học (1-5) nếu không có tiết học nào thì được coi là rảnh.
        public static List<FreeTimeSlot> GetFreeTimeSlots(List<ClassSession> sessions)
        {
            var result = new List<FreeTimeSlot>();
            var allCourseCodes = sessions.SelectMany(s => s.CourseCodes).Distinct().ToList();
            var allRooms = sessions.SelectMany(s => s.Lessons).Select(l => l.Room).Distinct().ToList();

            // Xác định phạm vi ngày từ toàn bộ thời khóa biểu
            DateTime minDate = sessions
                .SelectMany(s => s.Lessons)
                .Min(l => l.StartDate);

            DateTime maxDate = sessions
                .SelectMany(s => s.Lessons)
                .Max(l => l.EndDate);

            // Duyệt từng ngày trong phạm vi
            for (DateTime date = minDate.Date; date <= maxDate.Date; date = date.AddDays(1))
            {
                int dayOfWeek = (int)date.DayOfWeek; // đã tương thích: CN=0, Thứ 2=1, ..., Thứ 7=6

                // Với mỗi nhóm tiết 1 đến 5
                for (int periodGroup = 1; periodGroup <= 5; periodGroup++)
                {
                    var slot = new FreeTimeSlot
                    {
                        Date = date,
                        PeriodGroup = periodGroup,
                        CourseFree = new Dictionary<string, bool>(),
                        RoomFree = new Dictionary<string, bool>()
                    };

                    // Mặc định tất cả rảnh
                    foreach (var code in allCourseCodes)
                        slot.CourseFree[code] = true;

                    foreach (var room in allRooms)
                        slot.RoomFree[room] = true;

                    // Kiểm tra các tiết học bận
                    foreach (var session in sessions)
                    {
                        bool isSessionBusy = session.Lessons.Any(lesson =>
                            date >= lesson.StartDate.Date &&
                            date <= lesson.EndDate.Date &&
                            lesson.DayOfWeek == dayOfWeek &&
                            IsLessonOverlappingPeriodGroup(lesson, periodGroup)
                        );

                        if (isSessionBusy)
                        {
                            foreach (var code in session.CourseCodes)
                                slot.CourseFree[code] = false;

                            foreach (var lesson in session.Lessons.Where(l => l.Room != ""))
                                slot.RoomFree[lesson.Room] = false;
                        }
                    }

                    result.Add(slot);
                }
            }

            return result;
        }

        public static List<ExamSession> GenerateExamSchedule(
            DateTime examStartDate,
            DateTime examEndDate,
            List<ClassSession> sessions,
            Dictionary<string, DateTime> subjectEndDates,
            Dictionary<string, HashSet<(DateTime, int)>> freeSlots)
        {
            var examSchedule = new List<ExamSession>();

            // 1. Nhóm các khóa học theo môn học
            var subjectToCourses = sessions
                .GroupBy(s => s.Subject)
                .ToDictionary(g => g.Key, g => g.Select(s => s.Course).Distinct().ToList());

            // 2. Danh sách phòng học
            var allRooms = sessions.Select(s => s.Room).Distinct().ToList();

            // 3. Danh sách môn học cần thi
            var subjects = subjectToCourses.Keys
                .OrderBy(sub => subjectEndDates.ContainsKey(sub) ? subjectEndDates[sub] : DateTime.MaxValue)
                .ToList();

            var usedSlots = new HashSet<(DateTime, int, string)>(); // (Ngày, Nhóm tiết, Phòng)
            var courseExamDays = new Dictionary<string, HashSet<DateTime>>(); // khóa học → ngày đã thi

            foreach (var subject in subjects)
            {
                if (!subjectEndDates.ContainsKey(subject))
                    continue;

                DateTime lastClass = subjectEndDates[subject];
                DateTime earliestExamDate = lastClass.AddDays(2);
                if (earliestExamDate < examStartDate)
                    earliestExamDate = examStartDate;

                var courses = subjectToCourses[subject];

                bool scheduled = false;

                for (DateTime date = earliestExamDate; date <= examEndDate; date = date.AddDays(1))
                {
                    // Bỏ qua thứ 7, chủ nhật
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    for (int period = 1; period <= 4; period++)
                    {
                        foreach (var room in allRooms)
                        {
                            var slotKey = (date, period, room);
                            if (usedSlots.Contains(slotKey))
                                continue;

                            // Kiểm tra phòng có rảnh không
                            if (!freeSlots.ContainsKey(room) || !freeSlots[room].Contains((date, period)))
                                continue;

                            // Kiểm tra các khóa học có rảnh không tại thời điểm này
                            bool allCoursesAvailable = courses.All(course =>
                                freeSlots.ContainsKey(course) && freeSlots[course].Contains((date, period)) &&
                                (!courseExamDays.ContainsKey(course) || !courseExamDays[course].Contains(date))
                            );

                            if (!allCoursesAvailable)
                                continue;

                            // Ghi nhận kết quả
                            examSchedule.Add(new ExamSession
                            {
                                Subject = subject,
                                Courses = courses,
                                Date = date,
                                Period = period,
                                Room = room
                            });

                            usedSlots.Add(slotKey);
                            foreach (var course in courses)
                            {
                                if (!courseExamDays.ContainsKey(course))
                                    courseExamDays[course] = new HashSet<DateTime>();
                                courseExamDays[course].Add(date);
                            }

                            scheduled = true;
                            break;
                        }
                        if (scheduled) break;
                    }
                    if (scheduled) break;
                }
            }

            return examSchedule;
        }

        //Kiểm tra tiết có trùng nhóm không
        private static bool IsLessonOverlappingPeriodGroup(Lesson lesson, int group)
        {
            // Mỗi group gồm:
            // 1: 1–3, 2: 4–6, 3: 7–9, 4: 10–12, 5: 13–16
            int groupStart = (group - 1) * 3 + 1;
            int groupEnd = group == 5 ? 16 : groupStart + 2;

            return lesson.EndPeriod >= groupStart && lesson.StartPeriod <= groupEnd;
        }

    }
}
