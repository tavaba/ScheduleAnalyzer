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
            List<SubjectEndDate> subjectEndDates,
            List<FreeTimeSlot> freeSlots)
        {
            var result = new List<ExamSession>();

            // Bước 1: Xác định danh sách tất cả các khóa học và phòng học
            var allCourses = sessions.SelectMany(s => s.CourseCodes).Distinct().ToList();
            var allRooms = sessions.SelectMany(s => s.Lessons).Select(l => l.Room).Distinct().ToList();

            // Bước 2: Xây dựng danh sách các ngày thi hợp lệ (từ thứ Hai đến thứ Sáu)
            var examDates = Enumerable.Range(0, (examEndDate - examStartDate).Days + 1)
                .Select(offset => examStartDate.AddDays(offset))
                .Where(d => d.DayOfWeek >= DayOfWeek.Monday && d.DayOfWeek <= DayOfWeek.Friday)
                .ToList();

            // Bước 3: Xây dựng danh sách các nhóm tiết (PeriodGroup: 1 -> 5)
            var periodGroups = Enumerable.Range(1, 5).ToList();

            // Bước 4: Mở rộng freeSlots đến hết ngày thi (giả sử các ngày mới là trống hoàn toàn)
            var existingDates = freeSlots.Select(f => f.Date).Distinct().ToHashSet();

            foreach (var date in examDates)
            {
                if (!existingDates.Contains(date))
                {
                    foreach (var period in periodGroups)
                    {
                        var newSlot = new FreeTimeSlot
                        {
                            Date = date,
                            PeriodGroup = period,
                            CourseFree = allCourses.ToDictionary(c => c, c => true),
                            RoomFree = allRooms.ToDictionary(r => r, r => true)
                        };
                        freeSlots.Add(newSlot);
                    }
                }
            }

            // Bước 5: Sắp xếp danh sách môn học theo ngày kết thúc
            var sortedSubjects = subjectEndDates.OrderBy(s => s.EndDate).ToList();

            foreach (var subject in sortedSubjects)
            {
                var relatedClass = sessions.FirstOrDefault(c => c.SubjectName == subject.SubjectName);
                if (relatedClass == null) continue;

                var validExamStartDate = subject.EndDate.AddDays(2);

                var possibleSlots = freeSlots
                    .Where(f =>
                        f.Date >= validExamStartDate &&
                        f.Date >= examStartDate &&
                        f.Date <= examEndDate &&
                        f.Date.DayOfWeek != DayOfWeek.Saturday &&
                        f.Date.DayOfWeek != DayOfWeek.Sunday)
                    .OrderBy(f => f.Date)
                    .ThenBy(f => f.PeriodGroup)
                    .ToList();

                bool scheduled = false;

                foreach (var slot in possibleSlots)
                {
                    bool allCoursesFree = relatedClass.CourseCodes.All(c => slot.CourseFree.ContainsKey(c) && slot.CourseFree[c]);
                    var availableRoom = slot.RoomFree.FirstOrDefault(r => r.Value);

                    if (allCoursesFree && !string.IsNullOrEmpty(availableRoom.Key))
                    {
                        // Đánh dấu đã dùng
                        foreach (var c in relatedClass.CourseCodes)
                            slot.CourseFree[c] = false;

                        slot.RoomFree[availableRoom.Key] = false;

                        // Thêm phiên thi
                        result.Add(new ExamSession
                        {
                            Subject = subject.SubjectName,
                            Courses = new List<string>(relatedClass.CourseCodes),
                            Date = slot.Date,
                            Period = slot.PeriodGroup,
                            Room = availableRoom.Key
                        });

                        scheduled = true;
                        break;
                    }
                }

                if (!scheduled)
                {
                    throw new Exception($"Không thể xếp lịch thi cho môn '{subject.SubjectName}'.");
                }
            }

            return result;
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
