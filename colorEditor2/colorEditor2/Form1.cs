using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace colorEditor2
{
    public partial class Form1 : Form
    {
        
        // こんすとらくた
        public Form1()
        {
            InitializeComponent();
        }

        // 画像ファイル選択ボタンが押されたとき
        private void btnSelectInputFile_Click(object sender, EventArgs e)
        {
            txtInputFilePath.Text = ImageFileSelect();

            if (txtInputFilePath.Text.Length > 0)
            {
                int ind = txtInputFilePath.Text.LastIndexOf(".");

                txtOutputFilePath.Text = txtInputFilePath.Text.Substring(0, ind) + "_output.jpg";
            }

            
        }



        //ファイル選択ダイアログ表示
        private String ImageFileSelect()
        {
            //ファイルを開くダイアログボックスの作成  
            var ofd = new OpenFileDialog();
            //ファイルフィルタ  
            ofd.Filter = "Image File(*.bmp,*.jpg,*.png,*.tif)|*.bmp;*.jpg;*.png;*.tif|Bitmap(*.bmp)|*.bmp|Jpeg(*.jpg)|*.jpg|PNG(*.png)|*.png";
            //ダイアログの表示 （Cancelボタンがクリックされた場合は何もしない）
            if (ofd.ShowDialog() == DialogResult.Cancel) return null;

            return ofd.FileName;
        }



        // 指定されたファイルのBitmapをロードして返す。
        private Bitmap ImageFileOpen(string filePath)
        {
            // 指定したファイルが存在するか？確認
            if (System.IO.File.Exists(filePath) == false) return null;

            // 拡張子の確認
            var ext = System.IO.Path.GetExtension(filePath).ToLower();

            // ファイルの拡張子が対応しているファイルかどうか調べる
            if (
                (ext != ".bmp") &&
                (ext != ".jpg") &&
                (ext != ".png") &&
                (ext != ".tif")
                )
            {
                return null;
            }

            Bitmap bmp;

            // ファイルストリームでファイルを開く
            using (var fs = new System.IO.FileStream(
                filePath,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read))
            {
                bmp = new Bitmap(fs);
            }
            return bmp;
        }



        //Formに文字を別スレッドから表示させるデリゲート
        delegate void SetTextCallback(string text);

        //Formに文字を別スレッドから表示させるメソッド
        private void SetText(string text)
        {
            // コントロールが破棄されているならば、何もしない
            if (lblDescription.IsDisposed) return;

            // 呼び出し元のコントロールのスレッドと異なるかを確認する
            if (lblDescription.InvokeRequired)
            {
                // このブロックは、別スレッドで実行される

                // 同一メソッドへのコールバックを作成する
                SetTextCallback delegateMethod = new SetTextCallback(SetText);

                // コントロールのInvoke()メソッドを呼び出すことで、コントロールのスレッドでこのメソッドを実行する
                lblDescription.Invoke(delegateMethod, new object[] { text });
            }
            else
            {
                // このブロックは、同一スレッドで実行される

                // コントロールを直接呼び出す
                lblDescription.Text = text;
            }
        }



        // 編集開始ボタン押下したとき
        private void btnStartEdit_Click(object sender, EventArgs e)
        {
            if (txtInputFilePath.Text.Length == 0)
            {
                return;
            }

                // 別スレッドで動作
                System.Threading.Thread thread = new Thread(new ThreadStart(() =>
            {

                SetText("started.");


                Bitmap bmp = ImageFileOpen(txtInputFilePath.Text);

                for (int x = 0; x < bmp.Width; x++)
                {
                    
                    SetText("Converting... " + x + "/" + bmp.Width);
                    

                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color color1 = bmp.GetPixel(x, y);

                        int redColor = color1.R;
                        int greenColor = color1.G;
                        int blueColor = color1.B;

                        if (color1.R < color1.G)
                        {
                            //赤よりも緑が強いとき、入れ替え


                            redColor = color1.G;

                            greenColor = color1.R;

                            if (greenColor > 30)
                            {
                                greenColor -= 30;
                            }

                            if (redColor < 220)
                            {
                                redColor += 30;
                            }


                        }



                        Color newColor = Color.FromArgb(redColor, greenColor, blueColor);

                        bmp.SetPixel(x, y, newColor);

                        color1 = newColor;

                    }
                }

                //pictureBox1.Image = bmp;

                SetText("Finished.");

                bmp.Save(txtOutputFilePath.Text, System.Drawing.Imaging.ImageFormat.Jpeg);

                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", "/c " + txtOutputFilePath.Text);
                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;

                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;

                process = Process.Start(processStartInfo);
                process.WaitForExit();

            }));

            // スレッド起動
            thread.Start();

        }
    }
}
