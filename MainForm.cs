
using System;
using System.IO;
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
                Filter = "Excel files (*.xlsx)|*.xlsx"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtInputPath.Text = dlg.FileName;
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
            string input = txtInputPath.Text.Trim();
            string output = txtOutputPath.Text.Trim();

            if (!File.Exists(input))
            {
                MessageBox.Show("Tập tin thời khóa biểu không tồn tại.");
                return;
            }

            try
            {
                var sessions = TimetableReader.ReadSchedule(input);
                var subjectEndDates = TimetableProcessor.GetSubjectEndDates(sessions);
                var freeSlots = TimetableProcessor.GetFreeTimeSlots(sessions);

                // Lấy ngày bắt đầu và kết thúc thi từ giao diện
                DateTime examStartDate = dtpExamStartDate.Value.Date;
                DateTime examEndDate = dtpExamEndDate.Value.Date;

                if (examEndDate < examStartDate)
                {
                    MessageBox.Show("Ngày kết thúc không được sớm hơn ngày bắt đầu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Gọi hàm tạo lịch thi
                TimetableProcessor.GenerateExamSchedule(examStartDate, examEndDate, sessions, subjectEndDates, freeSlots);


                TimetableWriter.WriteResults(output, subjectEndDates, freeSlots);
                MessageBox.Show("Phân tích hoàn tất!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }
}
