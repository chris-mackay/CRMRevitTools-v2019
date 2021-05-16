//    Copyright(C) 2020 Christopher Ryan Mackay

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.If not, see<https://www.gnu.org/licenses/>.

namespace SharedParameterList
{
    partial class MainForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvParameters = new System.Windows.Forms.DataGridView();
            this.ElementId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Binding = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ParamType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GUID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ParamName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Family = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSaveFile = new System.Windows.Forms.Button();
            this.rbTypeParameters = new System.Windows.Forms.RadioButton();
            this.rbProjectParameters = new System.Windows.Forms.RadioButton();
            this.rbInstanceParameters = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParameters)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvParameters
            // 
            this.dgvParameters.AllowUserToAddRows = false;
            this.dgvParameters.AllowUserToDeleteRows = false;
            this.dgvParameters.AllowUserToOrderColumns = true;
            this.dgvParameters.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.MenuHighlight;
            this.dgvParameters.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvParameters.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvParameters.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvParameters.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.MenuHighlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvParameters.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvParameters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvParameters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ElementId,
            this.Binding,
            this.ParamType,
            this.GUID,
            this.ParamName,
            this.Family});
            this.dgvParameters.GridColor = System.Drawing.SystemColors.ControlLight;
            this.dgvParameters.Location = new System.Drawing.Point(12, 85);
            this.dgvParameters.Name = "dgvParameters";
            this.dgvParameters.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvParameters.RowHeadersVisible = false;
            this.dgvParameters.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.MenuHighlight;
            this.dgvParameters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvParameters.Size = new System.Drawing.Size(860, 536);
            this.dgvParameters.TabIndex = 3;
            this.dgvParameters.TabStop = false;
            this.dgvParameters.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dgvParameters_MouseUp);
            // 
            // ElementId
            // 
            this.ElementId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ElementId.HeaderText = "Element Id";
            this.ElementId.Name = "ElementId";
            this.ElementId.Width = 82;
            // 
            // Binding
            // 
            this.Binding.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Binding.HeaderText = "Binding";
            this.Binding.Name = "Binding";
            this.Binding.Width = 67;
            // 
            // ParamType
            // 
            this.ParamType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ParamType.HeaderText = "Parameter Type";
            this.ParamType.Name = "ParamType";
            this.ParamType.ReadOnly = true;
            this.ParamType.Width = 107;
            // 
            // GUID
            // 
            this.GUID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.GUID.HeaderText = "GUID";
            this.GUID.Name = "GUID";
            this.GUID.ReadOnly = true;
            this.GUID.Width = 59;
            // 
            // ParamName
            // 
            this.ParamName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ParamName.FillWeight = 92.8934F;
            this.ParamName.HeaderText = "Parameter Name";
            this.ParamName.Name = "ParamName";
            this.ParamName.ReadOnly = true;
            this.ParamName.Width = 111;
            // 
            // Family
            // 
            this.Family.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Family.HeaderText = "Family";
            this.Family.Name = "Family";
            this.Family.Width = 61;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(797, 627);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.TabStop = false;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // btnSaveFile
            // 
            this.btnSaveFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveFile.Location = new System.Drawing.Point(716, 627);
            this.btnSaveFile.Name = "btnSaveFile";
            this.btnSaveFile.Size = new System.Drawing.Size(75, 23);
            this.btnSaveFile.TabIndex = 4;
            this.btnSaveFile.Text = "Save File";
            this.btnSaveFile.UseVisualStyleBackColor = true;
            this.btnSaveFile.Click += new System.EventHandler(this.btnSaveFile_Click);
            // 
            // rbTypeParameters
            // 
            this.rbTypeParameters.AutoSize = true;
            this.rbTypeParameters.Location = new System.Drawing.Point(12, 35);
            this.rbTypeParameters.Name = "rbTypeParameters";
            this.rbTypeParameters.Size = new System.Drawing.Size(305, 17);
            this.rbTypeParameters.TabIndex = 1;
            this.rbTypeParameters.TabStop = true;
            this.rbTypeParameters.Text = "Type parameters (from Families loaded into Project Browser)";
            this.rbTypeParameters.UseVisualStyleBackColor = true;
            this.rbTypeParameters.CheckedChanged += new System.EventHandler(this.RadioButtonChecked);
            // 
            // rbProjectParameters
            // 
            this.rbProjectParameters.AutoSize = true;
            this.rbProjectParameters.Location = new System.Drawing.Point(12, 12);
            this.rbProjectParameters.Name = "rbProjectParameters";
            this.rbProjectParameters.Size = new System.Drawing.Size(484, 17);
            this.rbProjectParameters.TabIndex = 0;
            this.rbProjectParameters.TabStop = true;
            this.rbProjectParameters.Text = "Project Parameters (Type and Instance parameters that have been manually added to" +
    " the project)";
            this.rbProjectParameters.UseVisualStyleBackColor = true;
            this.rbProjectParameters.CheckedChanged += new System.EventHandler(this.RadioButtonChecked);
            // 
            // rbInstanceParameters
            // 
            this.rbInstanceParameters.AutoSize = true;
            this.rbInstanceParameters.Location = new System.Drawing.Point(12, 58);
            this.rbInstanceParameters.Name = "rbInstanceParameters";
            this.rbInstanceParameters.Size = new System.Drawing.Size(291, 17);
            this.rbInstanceParameters.TabIndex = 2;
            this.rbInstanceParameters.TabStop = true;
            this.rbInstanceParameters.Text = "Instance parameters (from Family Instances in the model)";
            this.rbInstanceParameters.UseVisualStyleBackColor = true;
            this.rbInstanceParameters.CheckedChanged += new System.EventHandler(this.RadioButtonChecked);
            // 
            // MainForm
            // 
            this.AcceptButton = this.btnSaveFile;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(884, 662);
            this.Controls.Add(this.rbInstanceParameters);
            this.Controls.Add(this.rbProjectParameters);
            this.Controls.Add(this.rbTypeParameters);
            this.Controls.Add(this.btnSaveFile);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.dgvParameters);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(620, 391);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shared Parameter List";
            ((System.ComponentModel.ISupportInitialize)(this.dgvParameters)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnClose;
        public System.Windows.Forms.DataGridView dgvParameters;
        private System.Windows.Forms.Button btnSaveFile;
        private System.Windows.Forms.RadioButton rbTypeParameters;
        private System.Windows.Forms.RadioButton rbProjectParameters;
        private System.Windows.Forms.RadioButton rbInstanceParameters;
        private System.Windows.Forms.DataGridViewTextBoxColumn ElementId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Binding;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParamType;
        private System.Windows.Forms.DataGridViewTextBoxColumn GUID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParamName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Family;
    }
}