using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataParallelismWithForEach
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource cancelToken = new CancellationTokenSource();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnProcessImage_Click(object sender, EventArgs e)
        {
            // Запустить новую "задачу" для обработки файлов.
            Task.Factory.StartNew(() =>
            {
                ProcessFiles();
            });
        }
        private void ProcessFiles()
        {
            // Использовать экземпляр ParallelOptions для хранения CancellationToken.
            ParallelOptions parOpts = new ParallelOptions();
            parOpts.CancellationToken = cancelToken.Token;
            parOpts.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            // Загрузить все файлы *.jpg и создать новую папку для модифицированных данных,
            string[] files = Directory.GetFiles
            (@"C:\Users\Nurdaulet\Pictures\Raptr Screenshots", "*.jpg",
            SearchOption.AllDirectories);
            string newDir = @"C:\ModifiedPictures";
            Directory.CreateDirectory(newDir);
            try
            {
                // Обработать данные изображения в параллельном режиме!
                Parallel.ForEach(files, parOpts, currentFile =>
                {
                    parOpts.CancellationToken.ThrowIfCancellationRequested();
                    string filename = Path.GetFileName(currentFile);
                    using (Bitmap bitmap = new Bitmap(currentFile))
                    {
                        bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        bitmap.Save(Path.Combine(newDir, filename));
                        this.Invoke((Action)delegate
                        {
                            this.Text = string.Format("Processing {0} on thread {1}", filename,
                            Thread.CurrentThread.ManagedThreadId);
                        }
                        );
                    }
                });
            }
            catch (OperationCanceledException ex)
            {
                this.Invoke((Action)delegate
                {
                    this.Text = ex.Message;
                });
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cancelToken.Cancel();
        }
    }
}
