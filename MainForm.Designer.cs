
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
        private System.Windows.Forms.Button btnAnalyze;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblInput = new System.Windows.Forms.Label();
            txtInputPath = new System.Windows.Forms.TextBox();
            btnBrowseInput = new System.Windows.Forms.Button();
            lblOutput = new System.Windows.Forms.Label();
            txtOutputPath = new System.Windows.Forms.TextBox();
            btnBrowseOutput = new System.Windows.Forms.Button();
            btnAnalyze = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // lblInput
            // 
            lblInput.AutoSize = true;
            lblInput.Location = new System.Drawing.Point(12, 15);
            lblInput.Name = "lblInput";
            lblInput.Size = new System.Drawing.Size(125, 15);
            lblInput.TabIndex = 0;
            lblInput.Text = "Tập tin thời khóa biểu:";
            // 
            // txtInputPath
            // 
            txtInputPath.Location = new System.Drawing.Point(143, 12);
            txtInputPath.Name = "txtInputPath";
            txtInputPath.Size = new System.Drawing.Size(387, 23);
            txtInputPath.TabIndex = 1;
            txtInputPath.Text = "D:\\Công việc\\Xây dựng lịc thi KTMM\\Thời khóa biểu.xlsx";
            // 
            // btnBrowseInput
            // 
            btnBrowseInput.Location = new System.Drawing.Point(540, 11);
            btnBrowseInput.Name = "btnBrowseInput";
            btnBrowseInput.Size = new System.Drawing.Size(30, 25);
            btnBrowseInput.TabIndex = 2;
            btnBrowseInput.Text = "...";
            btnBrowseInput.Click += btnBrowseInput_Click;
            // 
            // lblOutput
            // 
            lblOutput.AutoSize = true;
            lblOutput.Location = new System.Drawing.Point(12, 50);
            lblOutput.Name = "lblOutput";
            lblOutput.Size = new System.Drawing.Size(108, 15);
            lblOutput.TabIndex = 3;
            lblOutput.Text = "Tập tin lưu kết quả:";
            // 
            // txtOutputPath
            // 
            txtOutputPath.Location = new System.Drawing.Point(143, 47);
            txtOutputPath.Name = "txtOutputPath";
            txtOutputPath.Size = new System.Drawing.Size(387, 23);
            txtOutputPath.TabIndex = 4;
            txtOutputPath.Text = "D:\\tkb-h.xlsx";
            // 
            // btnBrowseOutput
            // 
            btnBrowseOutput.Location = new System.Drawing.Point(540, 46);
            btnBrowseOutput.Name = "btnBrowseOutput";
            btnBrowseOutput.Size = new System.Drawing.Size(30, 25);
            btnBrowseOutput.TabIndex = 5;
            btnBrowseOutput.Text = "...";
            btnBrowseOutput.Click += btnBrowseOutput_Click;
            // 
            // btnAnalyze
            // 
            btnAnalyze.Location = new System.Drawing.Point(250, 90);
            btnAnalyze.Name = "btnAnalyze";
            btnAnalyze.Size = new System.Drawing.Size(100, 30);
            btnAnalyze.TabIndex = 6;
            btnAnalyze.Text = "Phân tích";
            btnAnalyze.Click += btnAnalyze_Click;
            // 
            // MainForm
            // 
            ClientSize = new System.Drawing.Size(600, 140);
            Controls.Add(lblInput);
            Controls.Add(txtInputPath);
            Controls.Add(btnBrowseInput);
            Controls.Add(lblOutput);
            Controls.Add(txtOutputPath);
            Controls.Add(btnBrowseOutput);
            Controls.Add(btnAnalyze);
            Name = "MainForm";
            Text = "Phân tích thời khóa biểu KTMM";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
