using SixLabors.ImageSharp;
using System.Threading.Tasks;

namespace EInkService.Plugins
{
    public interface IPlugin
    {
        public Task DrawAsync(Image image, int width, int height, Theme theme);
    }
}
