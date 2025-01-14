﻿using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TNBase.Objects;

namespace TNBase.Forms.Scanning
{
    public partial class MagazinesScanInForm : Form
    {
        private enum ScanStatus
        {
            Ok,
            Error
        }

        private readonly List<Scan> scans = new List<Scan>();
        private ScanTypes scanType;
        private WalletTypes walletType;
        private IEnumerable<int> validWallets;
        private IEnumerable<int> stoppedWallets;

        public IEnumerable<Scan> Scans => scans;
        public bool ShouldScanOut { get; private set; }

        public MagazinesScanInForm()
        {
            InitializeComponent();
        }

        private void txtScannerInput_TextChanged(object sender, EventArgs e)
        {
            if (txtScannerInput.Text.Length > 5)
            {
                Scan();
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            string result = Interaction.InputBox("Scan barcode to remove from the list", "Remove Scan");
            if (!string.IsNullOrWhiteSpace(result))
            {
                int.TryParse(result, out int wallet);
                var scan = scans.LastOrDefault(x => x.Wallet == wallet);
                if (scan != null)
                {
                    scans.Remove(scan);
                    UpdateScanList(wallet);
                }
            }
            txtScannerInput.Focus();
        }

        private void btnScanOut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            ShouldScanOut = true;
            Close();
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void txtScannerInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                if (!string.IsNullOrWhiteSpace(txtScannerInput.Text))
                    Scan();
            }
        }

        private void Scan()
        {
            int.TryParse(txtScannerInput.Text, out int wallet);
            if (wallet > 0)
            {
                scans.Add(new Scan
                {
                    Wallet = wallet,
                    ScanType = scanType,
                    WalletType = walletType
                });

                if (validWallets.Contains(wallet))
                {
                    var count = scans.Count(x => x.Wallet == wallet);
                    if (count > 1)
                    {
                        ModuleSounds.DoubleBeep();
                        SetStatusMessage($"Repeat scan for wallet {wallet}.", ScanStatus.Error);
                    }
                    else
                    {
                        ModuleSounds.PlayBeep();
                        SetStatusMessage($"Last scanned {wallet}.", ScanStatus.Ok);
                    }
                }
                else if (stoppedWallets.Contains(wallet))
                {
                    ModuleSounds.PlayStopped();
                    SetStatusMessage($"Wallet {wallet} is paused.", ScanStatus.Error);
                }
                else
                {
                    ModuleSounds.PlayNotInUse();
                    SetStatusMessage($"Wallet {wallet} not in use.", ScanStatus.Error);
                }

                UpdateScanList(wallet);
            }
            else
            {
                ModuleSounds.BeepInvalid();
                SetStatusMessage($"Invalid barcode {txtScannerInput.Text}.", ScanStatus.Error);
            }

            txtScannerInput.Text = "";
        }

        private void SetStatusMessage(string message, ScanStatus status)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = status == ScanStatus.Ok ? System.Drawing.SystemColors.ControlDarkDark : System.Drawing.Color.Red;
        }

        private void UpdateScanList(int lastScaned)
        {
            lstScanned.Items.Clear();
            lstScanned.Items.AddRange(scans
                .GroupBy(x => x.Wallet)
                .Select(x => new ListViewItem(new string[] { x.First().Wallet.ToString(), x.Count().ToString() })
                {
                    Selected = lastScaned == x.First().Wallet
                })
                .ToArray());
        }

        private void FormMagazineScanIn_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK && scans.Any())
            {
                var result = MessageBox.Show("Closing this form will discard all the scans. Do you want to continue?", "Close", MessageBoxButtons.OKCancel);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        public void Setup(string title, ScanTypes scanType, WalletTypes walletType, IEnumerable<int> validWallets, IEnumerable<int> stoppedWallets)
        {
            this.scanType = scanType;
            this.walletType = walletType;
            this.validWallets = validWallets;
            this.stoppedWallets = stoppedWallets;

            Text = title;
            ListLabel.Text = $"Scanned {walletType.ToString().ToLower()} wallets:";
            ScanInputLabel.Text = $"Please scan {scanType.ToString().ToLower()} a wallet:";
            lblStatus.Text = "";

            if (scanType == ScanTypes.OUT)
            {
                btnScanOut.Visible = false;
            }
        }

        private void lstScanned_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar))
            {
                txtScannerInput.Focus();
                txtScannerInput.Text = txtScannerInput.Text + e.KeyChar;
                txtScannerInput.SelectionStart = txtScannerInput.Text.Length;
            }
        }
    }
}
