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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfTextBenchmark
{
	public class TextBlockMatrixView : MatrixView
	{
		protected readonly Canvas host = new Canvas();
		
		public TextBlockMatrixView()
		{
			this.AddVisualChild(host);
			this.AddLogicalChild(host);
		}
		
		public override string ToString()
		{
			return "Canvas + TextBlocks";
		}
		
		protected override int VisualChildrenCount {
			get { return 1; }
		}
		
		protected override Visual GetVisualChild(int index)
		{
			return host;
		}
		
		protected override Size MeasureOverride(Size availableSize)
		{
			host.Measure(availableSize);
			return host.DesiredSize;
		}
		
		protected override Size ArrangeOverride(Size finalSize)
		{
			host.Arrange(new Rect(new Point(0, 0), finalSize));
			return finalSize;
		}
		
		public override void Refresh()
		{
			Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate { OnFrameRendered(EventArgs.Empty); }));
		}
		
		public override IText AddText(string text, double fontSize, SolidColorBrush brush)
		{
			MyText myText = new MyText();
			myText.Text = text;
			myText.FontSize = fontSize;
			myText.Foreground = brush;
			myText.parentCanvas = host;
			host.Children.Add(myText);
			return myText;
		}
		
		sealed class MyText : TextBlock, IText
		{
			public Canvas parentCanvas;
			
			public Point Position {
				get {
					return new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
				}
				set {
					Canvas.SetLeft(this, value.X);
					Canvas.SetTop(this, value.Y);
				}
			}
			
			public void Remove()
			{
				parentCanvas.Children.Remove(this);
			}
		}
	}
}
