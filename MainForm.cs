
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ScheduleAnalyzer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls",
                Title = "Chọn một hoặc nhiều tập tin thời khóa biểu",
                Multiselect = true
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Gộp đường dẫn theo dạng: "D:\file1.xlsx";"D:\file2.xls"
                var quotedPaths = dlg.FileNames.Select(path => $"\"{path}\"");
                txtInputPath.Text = string.Join(";", quotedPaths);
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtOutputPath.Text = dlg.FileName;
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            // Lấy ngày bắt đầu và kết thúc thi từ giao diện
            DateTime examStartDate = dtpExamStartDate.Value.Date;
            DateTime examEndDate = dtpExamEndDate.Value.Date;
            if (examEndDate < examStartDate)
            {
                MessageBox.Show("Ngày kết thúc không được sớm hơn ngày bắt đầu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string input = txtInputPath.Text.Trim();
            string output = txtOutputPath.Text.Trim();

            try
            {
                IWorkbook workbook = new XSSFWorkbook();
                var sessions = TimetableReader.ReadSchedules(input);

                //Ngày kết thúc các môn học
                var subjectEndDates = TimetableProcessor.GetSubjectEndDates(sessions);
                TimetableWriter.WriteEndDates(workbook, subjectEndDates);

                //Thời gian rỗi của các khóa học và phòng học
                var freeTimeSlots = TimetableProcessor.GetFreeTimeSlots(sessions);
                TimetableWriter.WriteBusynesses(workbook, freeTimeSlots);

                //Lịch thi
                var examSessions = TimetableProcessor.GenerateExamSchedule(examStartDate, examEndDate, sessions, subjectEndDates, freeTimeSlots);
                TimetableWriter.WriteExamSchedule(workbook, examSessions);

                //Ghi file
                using (FileStream fs = new FileStream(output, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fs);
                }

                MessageBox.Show("Phân tích hoàn tất!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }
}
