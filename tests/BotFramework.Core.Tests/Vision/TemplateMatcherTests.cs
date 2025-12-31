using System.Drawing;
using BotFramework.Core.Vision;

using Moq;
namespace BotFramework.Core.Tests.Vision;

[TestClass]
public sealed class TemplateMatcherTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    private static string GetRepositoryRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
        {
            var gitDirectories = Directory.GetDirectories(directory, ".git");
            var gitIgnoreFiles = Directory.GetFiles(directory, ".gitignore");
            if (gitDirectories.Length > 0 && gitIgnoreFiles.Length > 0)
            {
                return directory;
            }
            
            directory = Path.GetDirectoryName(directory);
        }

        throw new DirectoryNotFoundException("Could not find the repository root directory.");
    }

    [TestMethod]
    public void MatchTemplate_WithExactMatch_ReturnsHighSimilarity()
    {
        // Arrange
        var matcher = new TemplateMatcher(Mock.Of<IScreenCapturer>(), $"{RepositoryRoot}/.work");
        using var image = LoadTestResourceImage("incubator_egg_status_1440.png");
        using var template = LoadTestResourceImage("incubator_egg_status_stamina_1440.png");

        // Act
        var (location, similarity) = matcher.MatchTemplate(image, template, standardBounds: false);

        // Assert
        Assert.IsGreaterThan(0.90, similarity, "Similarity should be greater than 0.9");
        Assert.IsLessThanOrEqualTo(1.0, similarity, "Similarity should be less than or equal to 1.0");
        Assert.IsGreaterThanOrEqualTo(0, location.X, "Location X should be non-negative");
        Assert.IsGreaterThanOrEqualTo(0, location.Y, "Location Y should be non-negative");
    }

    [TestMethod]
    public void MatchTemplate_WithStandardBounds_AppliesColorFiltering()
    {
        // Arrange
        var matcher = new TemplateMatcher(Mock.Of<IScreenCapturer>(), $"{RepositoryRoot}/.work");
        using var image = LoadTestResourceImage("incubator_egg_status_1080.png");
        using var template = LoadTestResourceImage("incubator_egg_status_stamina_1080.png");

        // Act - with standard bounds (color filtering)
        var (_, similarityFalse) = matcher.MatchTemplate(image, template, standardBounds: false);
        var (_, similarityTrue) = matcher.MatchTemplate(image, template, standardBounds: true);

        // Assert - Results may differ due to color filtering
        Assert.IsGreaterThan(0.8, similarityTrue, "Similarity with standard bounds should be in valid range");
    }

    [TestMethod]
    public void MatchTemplate_WithIdenticalImages_ReturnsNearPerfectMatch()
    {
        // Arrange - Create a simple test bitmap programmatically
        var matcher = new TemplateMatcher(Mock.Of<IScreenCapturer>(), $"{RepositoryRoot}/.work");
        using var image = CreateSimpleTestBitmap(100, 100, Color.Blue);
        using var template = CreateSimpleTestBitmap(100, 100, Color.Blue);

        // Act
        var (location, similarity) = matcher.MatchTemplate(image, template, standardBounds: false);

        // Assert
        Assert.IsGreaterThan(0.99, similarity, $"Identical images should have very high similarity. Got: {similarity}");
        Assert.AreEqual(0, location.X, "Location should be at origin for identical images");
        Assert.AreEqual(0, location.Y, "Location should be at origin for identical images");
    }

    [TestMethod]
    public void MatchTemplate_WithSmallerTemplate_FindsLocationInLargerImage()
    {
        // Arrange - Create image with template embedded
        // TemplateMatcher compares gray-scale images by default, so saturation and brightness differences are more relevant than color
        var matcher = new TemplateMatcher(Mock.Of<IScreenCapturer>(), $"{RepositoryRoot}/.work");
        using var image = CreateSimpleTestBitmap(200, 200, Color.Black);
        using var template = CreateSimpleTestBitmap(50, 50, Color.Red);

        // Draw the template onto the image at a specific location
        using (var graphics = Graphics.FromImage(image))
        {
            graphics.DrawImage(template, new System.Drawing.Point(75, 75));
        }

        // Act
        var (location, similarity) = matcher.MatchTemplate(image, template, standardBounds: false);

        // Assert
        Assert.IsGreaterThan(0.9, similarity, $"Template should be found with high confidence. Got: {similarity}");

        // Location should be near (75, 75) where we placed it
        Assert.IsLessThanOrEqualTo(5, Math.Abs(location.X - 75), $"X location should be near 75. Got: {location.X}");
        Assert.IsLessThanOrEqualTo(5, Math.Abs(location.Y - 75), $"Y location should be near 75. Got: {location.Y}");
    }

    [TestMethod]
    public void MatchTemplate_WithNonMatchingImages_ReturnsLowSimilarity()
    {
        // Arrange
        var matcher = new TemplateMatcher(Mock.Of<IScreenCapturer>(), $"{RepositoryRoot}/.work");
        using var image = CreateSimpleTestBitmap(300, 300, Color.Red);
        using var template = LoadTestResourceImage("walrus.png");

        // Act
        var (location, similarity) = matcher.MatchTemplate(image, template, standardBounds: false);

        // Assert
        Assert.IsLessThan(0.5, similarity, $"Non-matching images should have low similarity. Got: {similarity}");
    }

    private static Bitmap LoadTestResourceImage(string path)
    {
        string imagePath = Path.Combine(RepositoryRoot, "tests", "resources", path);

        return File.Exists(imagePath) ? new Bitmap(imagePath) : throw new FileNotFoundException($"Test resource not found: {imagePath}");
    }

    /// <summary>
    /// Helper method to create a simple solid-color bitmap for testing
    /// </summary>
    private static Bitmap CreateSimpleTestBitmap(int width, int height, Color color)
    {
        var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        using var brush = new SolidBrush(color);
        graphics.FillRectangle(brush, 0, 0, width, height);
        return bitmap;
    }
}
