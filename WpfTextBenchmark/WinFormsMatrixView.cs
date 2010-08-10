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
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace WpfTextBenchmark
{
	public abstract class WinFormsMatrixView : MatrixView
	{
		protected readonly WindowsFormsHost host = new WindowsFormsHost();
		
		public WinFormsMatrixView()
		{
			this.AddVisualChild(host);
			this.AddLogicalChild(host);
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
			host.Child.Invalidate();
		}
	}
}
