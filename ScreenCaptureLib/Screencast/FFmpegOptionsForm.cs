﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (C) 2008-2014 ShareX Developers

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using HelpersLib;
using ScreenCaptureLib;
using SevenZip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ScreenCaptureLib
{
    public partial class FFmpegOptionsForm : Form
    {
        public ScreencastOptions Options = new ScreencastOptions();
        public string DefaultToolsPath;

        public FFmpegOptionsForm(FFmpegOptions ffMpegOptions)
        {
            Options.FFmpeg = ffMpegOptions;

            InitializeComponent();

            this.Text = string.Format("{0} - FFmpeg Options", Application.ProductName);
            this.Icon = ShareXResources.Icon;
        }

        public FFmpegOptionsForm(ScreencastOptions options)
            : this(options.FFmpeg)
        {
            Options = options;

            if (options != null)
            {
                LoadSettings();
                UpdateUI();
            }
        }

        private void LoadSettings()
        {
            cbCodec.Items.AddRange(Helpers.GetEnumDescriptions<FFmpegVideoCodec>());
            cbCodec.SelectedIndex = (int)Options.FFmpeg.VideoCodec;
            cbCodec.SelectedIndexChanged += (sender, e) => UpdateUI();

            cbExtension.Text = Options.FFmpeg.Extension;
            cbExtension.SelectedIndexChanged += (sender, e) => UpdateUI();

            nudCRF.Value = Options.FFmpeg.CRF.Between((int)nudCRF.Minimum, (int)nudCRF.Maximum);
            nudCRF.ValueChanged += (sender, e) => UpdateUI();

            cbPreset.Items.AddRange(Helpers.GetEnumDescriptions<FFmpegPreset>());
            cbPreset.SelectedIndex = (int)Options.FFmpeg.Preset;
            cbPreset.SelectedIndexChanged += (sender, e) => UpdateUI();

            nudQscale.Value = Options.FFmpeg.qscale.Between((int)nudQscale.Minimum, (int)nudQscale.Maximum);
            nudQscale.ValueChanged += (sender, e) => UpdateUI();

            tbUserArgs.Text = Options.FFmpeg.UserArgs;
            tbUserArgs.TextChanged += (sender, e) => UpdateUI();

            string cli = "ffmpeg.exe";
            if (string.IsNullOrEmpty(Options.FFmpeg.CLIPath) && File.Exists(cli))
            {
                Options.FFmpeg.CLIPath = cli;
            }

            tbFFmpegPath.Text = Options.FFmpeg.CLIPath;
            tbFFmpegPath.TextChanged += (sender, e) => UpdateUI();
        }

        public void SaveSettings()
        {
            Options.FFmpeg.VideoCodec = (FFmpegVideoCodec)cbCodec.SelectedIndex;
            Options.FFmpeg.Extension = cbExtension.Text;

            Options.FFmpeg.CRF = (int)nudCRF.Value;
            Options.FFmpeg.Preset = (FFmpegPreset)cbPreset.SelectedIndex;

            Options.FFmpeg.qscale = (int)nudQscale.Value;

            Options.FFmpeg.UserArgs = tbUserArgs.Text;

            Options.FFmpeg.CLIPath = tbFFmpegPath.Text;
        }

        public void UpdateUI()
        {
            SaveSettings();

            gbH263.Visible = Options.FFmpeg.VideoCodec == FFmpegVideoCodec.libxvid;
            gbH264.Visible = Options.FFmpeg.VideoCodec == FFmpegVideoCodec.libx264 || Options.FFmpeg.VideoCodec == FFmpegVideoCodec.libvpx;

            if ((Options.FFmpeg.VideoCodec == FFmpegVideoCodec.libx264 || Options.FFmpeg.VideoCodec == FFmpegVideoCodec.libxvid) && cbExtension.Text == "webm")
            {
                cbExtension.Text = "mp4";
            }
            else if (Options.FFmpeg.VideoCodec == FFmpegVideoCodec.libvpx && cbExtension.Text != "webm")
            {
                cbExtension.Text = "webm";
            }

            tbCommandLinePreview.Text = Options.GetFFmpegArgs();
        }

        private void buttonFFmpegBrowse_Click(object sender, EventArgs e)
        {
            Helpers.BrowseFile("Browse for ffmpeg.exe", tbFFmpegPath, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            UpdateUI();
        }

        private void buttonFFmpegHelp_Click(object sender, EventArgs e)
        {
            Helpers.OpenURL("https://www.ffmpeg.org/ffmpeg.html");
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            FFmpegHelper.DownloadFFmpeg(true, DownloaderForm_InstallRequested);
        }

        private void DownloaderForm_InstallRequested(string filePath)
        {
            string extractPath = DefaultToolsPath ?? "ffmpeg.exe";
            bool result = FFmpegHelper.ExtractFFmpeg(filePath, extractPath);

            if (result)
            {
                this.InvokeSafe(() => tbFFmpegPath.Text = extractPath);
                MessageBox.Show("FFmpeg successfully downloaded.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Download of FFmpeg failed.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}