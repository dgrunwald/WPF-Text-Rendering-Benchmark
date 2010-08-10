/* 
 * Copyright (C) 2010 Daniel Grunwald
 *
 * This sourcecode is licenced under The GNU Lesser General Public License
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN
 * NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfTextBenchmark
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		MatrixView v ;
		Random rnd = new Random(1337);
		const int width = 950;
		const int height = 350;
		List<IText> texts = new List<IText>();
		IText framerateText;
		string[] possibleTexts;
		
		public Window1()
		{
			possibleTexts = Enumerable.Range(0, char.MaxValue).Select(c => (char)c).Where(char.IsLetterOrDigit).Select(c => c.ToString()).ToArray();
			InitializeComponent();
			Direct2DButton.IsEnabled = Environment.OSVersion.Version.Major >= 6;
		}

		void InitView()
		{
			this.Title = "Text Benchmark";
			Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(
				delegate {
					if (v != null)
						this.Title = "Text Benchmark - " + v.ToString();
				}));
			framerateText = null;
			stopwatch = null;
			texts.Clear();
			rnd = new Random(1337);
			for (int i = 0; i < (int)textCount.Value; i++) {
				texts.Add(NewRandomText());
				texts[i].Position = new Point(rnd.Next(0, width), rnd.Next(-50, height));
			}
			v.Width = width;
			v.Height = height;
			scroll.Content = v;
			frameCounter = 0;
			lastFrame.Restart();
			v.FrameRendered += new EventHandler(v_FrameRendered);
			Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(NextFrame));
		}
		
		int frameCounter;
		
		void v_FrameRendered(object sender, EventArgs e)
		{
			frameCounter++;
			NextFrame();
		}
		
		string RandomCharacter()
		{
			if (unicode.IsChecked == true)
				return possibleTexts[rnd.NextDouble() < 0.2 ? rnd.Next(possibleTexts.Length) : rnd.Next(100)];
			else
				return possibleTexts[rnd.Next(26*2+10)];
		}
		
		IText NewRandomText()
		{
			string s;
			if (words.IsChecked == true) {
				int length = rnd.Next(1, 10);
				s = string.Empty;
				for (int i = 0; i < length; i++)
					s += RandomCharacter();
			} else {
				s = RandomCharacter();
			}
			var brush = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(0,200),(byte)rnd.Next(100,255),(byte)rnd.Next(0,200)));
			brush.Freeze();
			IText text = v.AddText(s,
			                       rnd.Next(10, 16),
			                       brush
			                      );
			return text;
		}
		
		Stopwatch stopwatch;
		Stopwatch lastFrame = Stopwatch.StartNew();
		
		void NextFrame()
		{
			if (stopped.IsChecked == false && v != null) {
				if (stopwatch == null)
					stopwatch = Stopwatch.StartNew();
				if (stopwatch.ElapsedMilliseconds > 1000) {
					if (framerateText != null) framerateText.Remove();
					double framerate = frameCounter / stopwatch.Elapsed.TotalSeconds;
					framerateText = v.AddText(framerate.ToString("f1"), 16, Brushes.White);
					stopwatch.Restart();
					frameCounter = 0;
				}
				long elapsed = lastFrame.ElapsedMilliseconds;
				lastFrame.Restart();
				for (int i = 0; i < texts.Count; i++) {
					texts[i].Position += new Vector(0, elapsed / 2f);
					if (texts[i].Position.Y > height) {
						double oldHeight = texts[i].Position.Y;
						if (keepEverything.IsChecked == false) {
							texts[i].Remove();
							texts[i] = NewRandomText();
						}
						texts[i].Position = new Point(rnd.Next(0,width), oldHeight - height - 50);
					}
				}
				v.Refresh();
			}
		}
		
		void GlyphRunButton_Click(object sender, RoutedEventArgs e)
		{
			v = new GlyphMatrixView();
			InitView();
		}
		
		void TextLineButton_Click(object sender, RoutedEventArgs e)
		{
			v = new TextLineMatrixView();
			InitView();
		}
		
		void FormattedTextButton_Click(object sender, RoutedEventArgs e)
		{
			v = new FormattedTextMatrixView();
			InitView();
		}
		
		void TextBlockButton_Click(object sender, RoutedEventArgs e)
		{
			v = new TextBlockMatrixView();
			InitView();
		}
		
		void Direct2DButton_Click(object sender, RoutedEventArgs e)
		{
			v = new Direct2DTextMatrixView();
			InitView();
		}
		
		void GDIButton_Click(object sender, RoutedEventArgs e)
		{
			v = new GDIMatrixView(true);
			InitView();
		}
		
		void GDIPlusButton_Click(object sender, RoutedEventArgs e)
		{
			v = new GDIMatrixView(false);
			InitView();
		}
		
		void ExtTextOutButton_Click(object sender, RoutedEventArgs e)
		{
			v = new ExtTextOutMatrixView();
			InitView();
		}
		
		void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (v == null)
				return;
			while (texts.Count < (int)e.NewValue) {
				texts.Add(NewRandomText());
				texts[texts.Count-1].Position = new Point(rnd.Next(0, width), rnd.Next(-50, height));
			}
			while (texts.Count > (int)e.NewValue) {
				texts[texts.Count-1].Remove();
				texts.RemoveAt(texts.Count-1);
			}
			v.Refresh();
		}
		
		void Stopped_Unchecked(object sender, RoutedEventArgs e)
		{
			NextFrame();
		}
	}
}