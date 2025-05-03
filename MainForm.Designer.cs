
using System.Drawing;
using System.Windows.Forms;

namespace ScheduleAnalyzer
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.Button btnBrowseInput;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.Button btnBrowseOutput;
        private DateTimePicker dtpExamStartDate;
        private DateTimePicker dtpExamEndDate;
        private System.Windows.Forms.Button btnAnalyze;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblInput = new Label();
            txtInputPath = new TextBox();
            btnBrowseInput = new Button();
            lblOutput = new Label();
            txtOutputPath = new TextBox();
            btnBrowseOutput = new Button();
            btnAnalyze = new Button();
            lblStart = new Label();
            dtpExamStartDate = new DateTimePicker();
            lblEnd = new Label();
            dtpExamEndDate = new DateTimePicker();
            chkOneSubjectForAll = new CheckBox();
            chkShowBusynessFromExamStartDate = new CheckBox();
            SuspendLayout();
            // 
            // lblInput
            // 
            lblInput.AutoSize = true;
            lblInput.Location = new Point(12, 15);
            lblInput.Name = "lblInput";
            lblInput.Size = new Size(125, 15);
            lblInput.TabIndex = 0;
            lblInput.Text = "Tập tin thời khóa biểu:";
            // 
            // txtInputPath
            // 
            txtInputPath.Location = new Point(143, 12);
            txtInputPath.Name = "txtInputPath";
            txtInputPath.Size = new Size(387, 23);
            txtInputPath.TabIndex = 1;
            txtInputPath.Text = "\"D:\\thoi-khoa-bieu.xlsx\"";
            // 
            // btnBrowseInput
            // 
            btnBrowseInput.Location = new Point(540, 11);
            btnBrowseInput.Name = "btnBrowseInput";
            btnBrowseInput.Size = new Size(30, 25);
            btnBrowseInput.TabIndex = 2;
            btnBrowseInput.Text = "...";
            btnBrowseInput.Click += btnBrowseInput_Click;
            // 
            // lblOutput
            // 
            lblOutput.AutoSize = true;
            lblOutput.Location = new Point(12, 50);
            lblOutput.Name = "lblOutput";
            lblOutput.Size = new Size(108, 15);
            lblOutput.TabIndex = 3;
            lblOutput.Text = "Tập tin lưu kết quả:";
            // 
            // txtOutputPath
            // 
            txtOutputPath.Location = new Point(143, 47);
            txtOutputPath.Name = "txtOutputPath";
            txtOutputPath.Size = new Size(387, 23);
            txtOutputPath.TabIndex = 4;
            txtOutputPath.Text = "D:\\lich-thi.xlsx";
            // 
            // btnBrowseOutput
            // 
            btnBrowseOutput.Location = new Point(540, 46);
            btnBrowseOutput.Name = "btnBrowseOutput";
            btnBrowseOutput.Size = new Size(30, 25);
            btnBrowseOutput.TabIndex = 5;
            btnBrowseOutput.Text = "...";
            btnBrowseOutput.Click += btnBrowseOutput_Click;
            // 
            // btnAnalyze
            // 
            btnAnalyze.Location = new Point(143, 200);
            btnAnalyze.Name = "btnAnalyze";
            btnAnalyze.Size = new Size(100, 30);
            btnAnalyze.TabIndex = 6;
            btnAnalyze.Text = "Phân tích";
            btnAnalyze.Click += btnAnalyze_Click;
            // 
            // lblStart
            // 
            lblStart.Location = new Point(12, 82);
            lblStart.Name = "lblStart";
            lblStart.Size = new Size(100, 23);
            lblStart.TabIndex = 6;
            lblStart.Text = "Ngày bắt đầu thi";
            // 
            // dtpExamStartDate
            // 
            dtpExamStartDate.Location = new Point(143, 76);
            dtpExamStartDate.Name = "dtpExamStartDate";
            dtpExamStartDate.Size = new Size(200, 23);
            dtpExamStartDate.TabIndex = 7;
            // 
            // lblEnd
            // 
            lblEnd.Location = new Point(12, 111);
            lblEnd.Name = "lblEnd";
            lblEnd.Size = new Size(100, 23);
            lblEnd.TabIndex = 8;
            lblEnd.Text = "Ngày kết thúc thi";
            // 
            // dtpExamEndDate
            // 
            dtpExamEndDate.Location = new Point(143, 105);
            dtpExamEndDate.Name = "dtpExamEndDate";
            dtpExamEndDate.Size = new Size(200, 23);
            dtpExamEndDate.TabIndex = 9;
            // 
            // chkOneSubjectForAll
            // 
            chkOneSubjectForAll.AutoSize = true;
            chkOneSubjectForAll.Location = new Point(143, 134);
            chkOneSubjectForAll.Name = "chkOneSubjectForAll";
            chkOneSubjectForAll.Size = new Size(207, 19);
            chkOneSubjectForAll.TabIndex = 10;
            chkOneSubjectForAll.Text = "Một môn cho tất cả các đối tượng";
            chkOneSubjectForAll.UseVisualStyleBackColor = true;
            // 
            // chkShowBusynessFromExamStartDate
            // 
            chkShowBusynessFromExamStartDate.AutoSize = true;
            chkShowBusynessFromExamStartDate.Checked = true;
            chkShowBusynessFromExamStartDate.CheckState = CheckState.Checked;
            chkShowBusynessFromExamStartDate.Location = new Point(143, 159);
            chkShowBusynessFromExamStartDate.Name = "chkShowBusynessFromExamStartDate";
            chkShowBusynessFromExamStartDate.Size = new Size(398, 19);
            chkShowBusynessFromExamStartDate.TabIndex = 10;
            chkShowBusynessFromExamStartDate.Text = "Hiển thị thông tin giờ rỗi của khóa học, phòng học từ ngày bắt đầu thi";
            chkShowBusynessFromExamStartDate.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            ClientSize = new Size(600, 242);
            Controls.Add(chkShowBusynessFromExamStartDate);
            Controls.Add(chkOneSubjectForAll);
            Controls.Add(lblInput);
            Controls.Add(txtInputPath);
            Controls.Add(btnBrowseInput);
            Controls.Add(lblOutput);
            Controls.Add(txtOutputPath);
            Controls.Add(btnBrowseOutput);
            Controls.Add(lblStart);
            Controls.Add(dtpExamStartDate);
            Controls.Add(lblEnd);
            Controls.Add(dtpExamEndDate);
            Controls.Add(btnAnalyze);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            Text = "Phân tích thời khóa biểu KTMM";
            ResumeLayout(false);
            PerformLayout();
        }
        private Label lblStart;
        private Label lblEnd;
        private CheckBox chkOneSubjectForAll;
        private CheckBox chkShowBusynessFromExamStartDate;
    }
}
