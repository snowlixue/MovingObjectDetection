using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.VideoSurveillance;
using Emgu.CV.BgSegm;
using Emgu.CV.Util;
namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        int i = 0;
        Capture _capture;//创建摄像头
        BackgroundSubtractor _motionDetect;//创建一个背景检测模型。
        Mat scr = new Mat();//更新输入图像。
        Mat mask = new Mat();//运动检测输出图像。
        MotionHistory _motionhistory ;//创建MotionHistory类。
        Rectangle Rect;
        CircleF Cursor_Circl1 = new CircleF(); CircleF Cursor_Circl2 = new CircleF();       //光标圈
        LineSegment2DF Line1 = new LineSegment2DF();
        LineSegment2DF Line2 = new LineSegment2DF();//十字光标
        public Form1()
        {
            InitializeComponent();

            _motionDetect = new BackgroundSubtractorMOG2();//默认参数实例化类。
            //_motionhistory = new MotionHistory(0.01, 0.05, 0.5);
            OpenFileDialog op = new OpenFileDialog();
            string path = "";
         
            _capture = new Capture("1.avi");//打开运行目录下的1.avi视频。
            if (_capture != null) //if camera capture has been successfully created
            {
                _motionhistory = new MotionHistory(
                    0.1, //运动持续时间。
                    0.5, //运动最长时间。
                    0.05); //运动最短时间。
                _capture.ImageGrabbed += frame;//捕捉帧触发事件
                _capture.Start();//开启捕捉真事件。
            }
        }
        void frame(object sender, EventArgs e)//捕捉帧进行线程函数
        {
             double sum_pix=0;
             double angle = 0;
             VectorOfRect rects = new VectorOfRect();  //创建VectorOfRect存储运动矩形。
             Mat _segMask = new Mat();//创建背景蒙版图片。
            _capture.Retrieve(scr);//获取帧数据。
           // CvInvoke.Resize(scr, scr, new Size(320, 480));
            _motionDetect.Apply(scr, mask);//进行运动检测。
            CvInvoke.MedianBlur(mask, mask, 5);//中值滤波。
            _motionhistory.Update(mask);//更新背景图片。
            _motionhistory.GetMotionComponents(_segMask, rects);//获取背景蒙版及运动矩形。
            for (int j = 0; j < rects.ToArray().Length;j++ )//遍历所有运动矩形。
            {
                if(rects[j].Width*rects[j].Height>1000)//删除一些较小的矩形采用面积的方式。
                {
                    CvInvoke.Rectangle(scr, rects[j], new MCvScalar(0, 0, 255));//在scr图像总绘制运动矩形。
                    _motionhistory.MotionInfo(_segMask, rects[j], out angle, out sum_pix);//指定矩阵获取运动的角度和像素值。
                    CvInvoke.PutText(scr, "angle : " + (int)angle, rects[j].Location, Emgu.CV.CvEnum.FontFace.HersheyComplex, 0.5, new MCvScalar(0, 255, 0));//绘制运动的角度。
                    
                }
               
            }


            imageBox1.Image = drar_rect(scr);

           // imageBox1.Image = scr;//显示图像。
            imageBox2.Image = mask;//显示运动检测输出图像。
        }

        private void imageBox2_Click(object sender, EventArgs e)
        {

        }
        Mat drar_rect(Mat m)
        {
            Rect.X = 1; Rect.Y = 1; Rect.Width = m.Width / 8; Rect.Height =m.Height / 8;
            Cursor_Circl1 = new CircleF(new Point(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height), 4);
            Cursor_Circl2 = new CircleF(new Point(Rect.X + Rect.Width, Rect.Y + Rect.Height / 2), 4);
            Line1.P1 = new PointF(Rect.Width / 2, Rect.Height / 2 - 4); Line1.P2 = new PointF(Rect.Width / 2, Rect.Height / 2 + 4);
            Line2.P1 = new PointF(Rect.Width / 2 - 4, Rect.Height / 2); Line2.P2 = new PointF(Rect.Width / 2 + 4, Rect.Height / 2);

            Image<Bgr, Byte> img = new Image<Bgr, Byte>(m.Bitmap);
            img.Draw(Line1, new Bgr(255, 0, 0), 1); img.Draw(Line2, new Bgr(255, 0, 0), 1);
            img.Draw(Rect, new Bgr(255, 0, 0), 1); img.Draw(Cursor_Circl1, new Bgr(0, 0, 255), 1); img.Draw(Cursor_Circl2, new Bgr(0, 0, 255), 1);

            return img.Mat;
            
        }
    }
}
