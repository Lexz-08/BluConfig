namespace ConfigTest
{
	partial class Main
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.console = new System.Windows.Forms.RichTextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// console
			// 
			this.console.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.console.BackColor = System.Drawing.Color.Black;
			this.console.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.console.Font = new System.Drawing.Font("Segoe UI", 12F);
			this.console.ForeColor = System.Drawing.Color.White;
			this.console.Location = new System.Drawing.Point(12, 12);
			this.console.Name = "console";
			this.console.ReadOnly = true;
			this.console.Size = new System.Drawing.Size(543, 336);
			this.console.TabIndex = 0;
			this.console.Text = "";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(10, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = " ";
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(567, 360);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.console);
			this.ForeColor = System.Drawing.Color.White;
			this.Name = "Main";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox console;
		private System.Windows.Forms.Label label1;
	}
}

