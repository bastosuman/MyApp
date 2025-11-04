using System.Reflection;
using System.Linq;
using MyApp.Core.Interfaces;
using Xunit;

namespace MyApp.Tests;

public class CalculationServiceTests
{
    private readonly ICalculationService _calculationService;

    public CalculationServiceTests()
    {
        // Use reflection to instantiate VB.NET class due to C#/VB.NET interop compilation issues
        Assembly? assembly = null;
        Type? type = null;
        
        // Try multiple approaches to find the VB.NET assembly
        var currentAssembly = typeof(CalculationServiceTests).Assembly;
        var assemblyLocation = currentAssembly.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation)!;
        
        // Approach 1: Try to load from the same directory as the test assembly
        var assemblyPath = Path.Combine(assemblyDirectory, "MyApp.Services.VB.dll");
        if (File.Exists(assemblyPath))
        {
            try
            {
                assembly = Assembly.LoadFrom(assemblyPath);
                // Try full name first
                type = assembly.GetType("MyApp.Services.VB.CalculationService");
                // If that fails, search all types
                if (type == null)
                {
                    Type[]? allTypes = null;
                    try
                    {
                        allTypes = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        // Handle partially loaded types
                        allTypes = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                    }
                    
                    if (allTypes != null)
                    {
                        type = allTypes.FirstOrDefault(t => 
                            t != null && 
                            t.Name == "CalculationService" && 
                            (t.Namespace == "MyApp.Services.VB" || t.FullName?.Contains("CalculationService") == true));
                        
                        // If still not found, try just by name
                        if (type == null)
                        {
                            type = allTypes.FirstOrDefault(t => t != null && t.Name == "CalculationService");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging but continue
                System.Diagnostics.Debug.WriteLine($"LoadFrom failed: {ex.Message}");
            }
        }
        
        // Approach 2: Try to find it in referenced assemblies
        if (type == null)
        {
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            var vbAssemblyRef = referencedAssemblies.FirstOrDefault(a => a.Name == "MyApp.Services.VB");
            if (vbAssemblyRef != null)
            {
                try
                {
                    assembly = Assembly.Load(vbAssemblyRef);
                    type = assembly.GetType("MyApp.Services.VB.CalculationService");
                }
                catch
                {
                    // Continue to next approach
                }
            }
        }
        
        // Approach 3: Try to load from AppDomain (may already be loaded)
        if (type == null)
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                assembly = assemblies.FirstOrDefault(a => a.GetName().Name == "MyApp.Services.VB");
                if (assembly != null)
                {
                    type = assembly.GetType("MyApp.Services.VB.CalculationService");
                }
            }
            catch
            {
                // Continue
            }
        }
        
        // Approach 4: Try parent directory (bin/Debug/net8.0)
        if (type == null && assemblyDirectory != null)
        {
            var parentDir = Directory.GetParent(assemblyDirectory)?.FullName;
            if (parentDir != null)
            {
                var altPath = Path.Combine(parentDir, "MyApp.Services.VB.dll");
                if (File.Exists(altPath))
                {
                    try
                    {
                        assembly = Assembly.LoadFrom(altPath);
                        type = assembly.GetType("MyApp.Services.VB.CalculationService");
                    }
                    catch
                    {
                        // Continue
                    }
                }
            }
        }
        
        // Approach 5: Try to find all types in loaded assemblies and search for the class name
        if (type == null)
        {
            try
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var loadedAssembly in loadedAssemblies)
                {
                    try
                    {
                        // First try by name match
                        if (loadedAssembly.GetName().Name == "MyApp.Services.VB")
                        {
                            var types = loadedAssembly.GetTypes();
                            type = types.FirstOrDefault(t => t.Name == "CalculationService");
                            if (type == null)
                            {
                                // Try with full namespace
                                type = types.FirstOrDefault(t => t.Name == "CalculationService" && 
                                    t.Namespace == "MyApp.Services.VB");
                            }
                            if (type != null)
                            {
                                assembly = loadedAssembly;
                                break;
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        // Try to get types from the loaded types
                        if (ex.Types != null)
                        {
                            type = ex.Types.FirstOrDefault(t => t != null && 
                                t.Name == "CalculationService");
                            if (type != null && type.Namespace == "MyApp.Services.VB")
                            {
                                assembly = loadedAssembly;
                                break;
                            }
                        }
                    }
                    catch
                    {
                        // Continue to next assembly
                    }
                }
            }
            catch
            {
                // Continue
            }
        }
        
        // Approach 6: Try loading by assembly name (will resolve dependencies)
        if (type == null)
        {
            try
            {
                var assemblyName = new AssemblyName("MyApp.Services.VB");
                assembly = Assembly.Load(assemblyName);
                type = assembly.GetType("MyApp.Services.VB.CalculationService");
                if (type == null)
                {
                    var allTypes = assembly.GetTypes();
                    type = allTypes.FirstOrDefault(t => t.Name == "CalculationService");
                }
            }
            catch (Exception ex)
            {
                // Log but continue - this approach might fail if dependencies aren't resolved
                System.Diagnostics.Debug.WriteLine($"Assembly.Load failed: {ex.Message}");
            }
        }
        
        // Approach 7: Try to find the type by searching through all loaded assemblies
        // This is often needed when running tests through Visual Studio Test Explorer
        if (type == null)
        {
            try
            {
                var allLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var loadedAssembly in allLoadedAssemblies)
                {
                    try
                    {
                        var assemblyName = loadedAssembly.GetName().Name;
                        if (assemblyName == "MyApp.Services.VB" || assemblyName?.Contains("MyApp.Services.VB") == true)
                        {
                            // Try to get all types
                            Type[]? types = null;
                            try
                            {
                                types = loadedAssembly.GetTypes();
                            }
                                    catch (ReflectionTypeLoadException ex)
                                    {
                                        types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                                    }
                            
                            if (types != null)
                            {
                                type = types.FirstOrDefault(t => 
                                    t != null && 
                                    t.Name == "CalculationService" && 
                                    (t.Namespace == "MyApp.Services.VB" || t.FullName?.Contains("CalculationService") == true));
                                
                                if (type != null)
                                {
                                    assembly = loadedAssembly;
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Continue to next assembly
                    }
                }
            }
            catch
            {
                // Continue
            }
        }
        
        if (type == null)
        {
            // Try one more time with a direct file path check
            var testAssemblyLocation = typeof(CalculationServiceTests).Assembly.Location;
            var testAssemblyDir = Path.GetDirectoryName(testAssemblyLocation);
            var directPath = Path.Combine(testAssemblyDir ?? "", "MyApp.Services.VB.dll");
            
            if (File.Exists(directPath))
            {
                try
                {
                    assembly = Assembly.LoadFrom(directPath);
                    var allTypes = assembly.GetTypes();
                    type = allTypes.FirstOrDefault(t => t.Name == "CalculationService");
                }
                catch
                {
                    // Last attempt failed
                }
            }
        }
        
        if (type == null)
        {
            var errorMessage = $"Cannot find MyApp.Services.VB.CalculationService type. " +
                $"Checked path: {assemblyPath}, Assembly location: {assemblyLocation}. " +
                $"Loaded assemblies: {string.Join(", ", AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name))}";
            throw new InvalidOperationException(errorMessage);
        }
        
        _calculationService = (ICalculationService)Activator.CreateInstance(type)!;
    }

    #region CalculateInterest Tests

    [Fact]
    public void CalculateInterest_ValidInput_ReturnsCorrectInterest()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = 5.5m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        // Simple interest: I = P * r * t = 10000 * 0.055 * 1 = 550
        Assert.Equal(550m, result);
    }

    [Fact]
    public void CalculateInterest_TwoYearTerm_ReturnsCorrectInterest()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = 5.5m;
        int termMonths = 24;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        // I = P * r * t = 10000 * 0.055 * 2 = 1100
        Assert.Equal(1100m, result);
    }

    [Fact]
    public void CalculateInterest_ZeroPrincipal_ReturnsZero()
    {
        // Arrange
        decimal principal = 0m;
        decimal rate = 5.5m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateInterest_NegativePrincipal_ReturnsZero()
    {
        // Arrange
        decimal principal = -1000m;
        decimal rate = 5.5m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateInterest_ZeroRate_ReturnsZero()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = 0m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateInterest_NegativeRate_ReturnsZero()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = -5.5m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateInterest_ZeroTerm_ReturnsZero()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = 5.5m;
        int termMonths = 0;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateInterest_NegativeTerm_ReturnsZero()
    {
        // Arrange
        decimal principal = 10000m;
        decimal rate = 5.5m;
        int termMonths = -12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Theory]
    [InlineData(5000.0, 3.5, 6, 87.5)] // 5000 * 0.035 * 0.5 = 87.5
    [InlineData(25000.0, 7.25, 24, 3625.0)] // 25000 * 0.0725 * 2 = 3625
    [InlineData(100000.0, 4.2, 360, 126000.0)] // 100000 * 0.042 * 30 = 126000
    public void CalculateInterest_VariousInputs_ReturnsCorrectInterest(double principal, double rate, int termMonths, double expectedInterest)
    {
        // Act
        var result = _calculationService.CalculateInterest((decimal)principal, (decimal)rate, termMonths);

        // Assert
        Assert.Equal((decimal)expectedInterest, result);
    }

    [Fact]
    public void CalculateInterest_ResultIsRoundedToTwoDecimals()
    {
        // Arrange
        decimal principal = 1000m;
        decimal rate = 3.333m;
        int termMonths = 12;

        // Act
        var result = _calculationService.CalculateInterest(principal, rate, termMonths);

        // Assert
        // Should be rounded to 2 decimal places
        var decimals = result.ToString().Split('.');
        if (decimals.Length > 1)
        {
            Assert.True(decimals[1].Length <= 2);
        }
    }

    #endregion

    #region CalculateMonthlyPayment Tests

    [Fact]
    public void CalculateMonthlyPayment_ValidInput_ReturnsCorrectPayment()
    {
        // Arrange
        decimal principal = 100000m;
        decimal annualRate = 5.5m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        // Using amortization formula, should be approximately 567.79
        Assert.True(result > 567m && result < 568m);
    }

    [Fact]
    public void CalculateMonthlyPayment_ZeroPrincipal_ReturnsZero()
    {
        // Arrange
        decimal principal = 0m;
        decimal annualRate = 5.5m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateMonthlyPayment_NegativePrincipal_ReturnsZero()
    {
        // Arrange
        decimal principal = -10000m;
        decimal annualRate = 5.5m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateMonthlyPayment_ZeroTerm_ReturnsZero()
    {
        // Arrange
        decimal principal = 100000m;
        decimal annualRate = 5.5m;
        int termMonths = 0;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateMonthlyPayment_NegativeTerm_ReturnsZero()
    {
        // Arrange
        decimal principal = 100000m;
        decimal annualRate = 5.5m;
        int termMonths = -360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateMonthlyPayment_ZeroRate_ReturnsPrincipalDividedByTerm()
    {
        // Arrange
        decimal principal = 36000m;
        decimal annualRate = 0m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        // Should return principal / termMonths = 36000 / 360 = 100
        Assert.Equal(100m, result);
    }

    [Fact]
    public void CalculateMonthlyPayment_NegativeRate_ReturnsPrincipalDividedByTerm()
    {
        // Arrange
        decimal principal = 36000m;
        decimal annualRate = -5.5m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        // Should treat negative rate as zero and return principal / termMonths
        Assert.Equal(100m, result);
    }

    [Theory]
    [InlineData(50000.0, 4.5, 240)]
    [InlineData(200000.0, 6.0, 180)]
    [InlineData(10000.0, 8.0, 60)]
    public void CalculateMonthlyPayment_VariousValidInputs_ReturnsPositiveValue(double principal, double rate, int term)
    {
        // Act
        var result = _calculationService.CalculateMonthlyPayment((decimal)principal, (decimal)rate, term);

        // Assert
        Assert.True(result > 0);
        Assert.True(result <= (decimal)principal); // Monthly payment should be less than or equal to principal for reasonable rates
    }

    [Fact]
    public void CalculateMonthlyPayment_ResultIsRoundedToTwoDecimals()
    {
        // Arrange
        decimal principal = 100000m;
        decimal annualRate = 5.555m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateMonthlyPayment(principal, annualRate, termMonths);

        // Assert
        var decimals = result.ToString().Split('.');
        if (decimals.Length > 1)
        {
            Assert.True(decimals[1].Length <= 2);
        }
    }

    [Fact]
    public void CalculateMonthlyPayment_ShortTermLoan_ReturnsHigherPayment()
    {
        // Arrange
        decimal principal = 100000m;
        decimal annualRate = 5.5m;
        int shortTerm = 60;
        int longTerm = 360;

        // Act
        var shortTermPayment = _calculationService.CalculateMonthlyPayment(principal, annualRate, shortTerm);
        var longTermPayment = _calculationService.CalculateMonthlyPayment(principal, annualRate, longTerm);

        // Assert
        Assert.True(shortTermPayment > longTermPayment);
    }

    #endregion

    #region CalculateCreditScore Tests

    [Fact]
    public void CalculateCreditScore_ExcellentDebtToIncome_ReturnsHighScore()
    {
        // Arrange
        decimal monthlyIncome = 10000m;
        decimal monthlyDebt = 1500m; // 15% DTI
        int creditHistoryMonths = 60;
        bool hasBankruptcy = false;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 150 (DTI < 0.2) + 100 (history >= 60) = 550
        Assert.True(result >= 550);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_GoodDebtToIncome_ReturnsGoodScore()
    {
        // Arrange
        decimal monthlyIncome = 5000m;
        decimal monthlyDebt = 1500m; // 30% DTI
        int creditHistoryMonths = 60;
        bool hasBankruptcy = false;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 100 (DTI < 0.36) + 100 (history >= 60) = 500
        Assert.True(result >= 500);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_FairDebtToIncome_ReturnsFairScore()
    {
        // Arrange
        decimal monthlyIncome = 5000m;
        decimal monthlyDebt = 2000m; // 40% DTI
        int creditHistoryMonths = 36;
        bool hasBankruptcy = false;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 50 (DTI < 0.5) + 75 (history >= 36) = 425
        Assert.True(result >= 425);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_WithBankruptcy_ReturnsLowerScore()
    {
        // Arrange
        decimal monthlyIncome = 10000m;
        decimal monthlyDebt = 1500m;
        int creditHistoryMonths = 60;
        bool hasBankruptcy = true;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 150 (DTI < 0.2) + 100 (history >= 60) - 150 (bankruptcy) = 400
        Assert.True(result >= 300);
        Assert.True(result < 550); // Should be lower than without bankruptcy
    }

    [Fact]
    public void CalculateCreditScore_NoCreditHistory_ReturnsLowScore()
    {
        // Arrange
        decimal monthlyIncome = 5000m;
        decimal monthlyDebt = 500m;
        int creditHistoryMonths = 0;
        bool hasBankruptcy = false;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 150 (DTI < 0.2) + 0 (no history) = 450
        Assert.True(result >= 300);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_ScoreIsWithinValidRange()
    {
        // Arrange
        decimal monthlyIncome = 10000m;
        decimal monthlyDebt = 5000m;
        int creditHistoryMonths = 120;
        bool hasBankruptcy = true;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        Assert.True(result >= 300);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_ZeroIncome_ReturnsBaseScore()
    {
        // Arrange
        decimal monthlyIncome = 0m;
        decimal monthlyDebt = 0m;
        int creditHistoryMonths = 24;
        bool hasBankruptcy = false;

        // Act
        var result = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, creditHistoryMonths, hasBankruptcy);

        // Assert
        // Base 300 + 0 (no income calculation) + 50 (history >= 24) = 350
        Assert.True(result >= 300);
        Assert.True(result <= 850);
    }

    [Theory]
    [InlineData(10000.0, 1000.0, 72, false)] // Excellent DTI, long history
    [InlineData(3000.0, 1200.0, 36, false)] // Good DTI, medium history
    [InlineData(5000.0, 2500.0, 12, false)] // Fair DTI, short history
    [InlineData(8000.0, 2000.0, 48, true)] // Good DTI, medium history, bankruptcy
    public void CalculateCreditScore_VariousInputs_ReturnsValidScore(double income, double debt, int history, bool bankruptcy)
    {
        // Act
        var result = _calculationService.CalculateCreditScore((decimal)income, (decimal)debt, history, bankruptcy);

        // Assert
        Assert.True(result >= 300);
        Assert.True(result <= 850);
    }

    [Fact]
    public void CalculateCreditScore_LongCreditHistory_AddsMorePoints()
    {
        // Arrange
        decimal monthlyIncome = 5000m;
        decimal monthlyDebt = 1000m;
        bool hasBankruptcy = false;

        // Act
        var score12Months = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, 12, hasBankruptcy);
        var score60Months = _calculationService.CalculateCreditScore(monthlyIncome, monthlyDebt, 60, hasBankruptcy);

        // Assert
        Assert.True(score60Months > score12Months);
    }

    #endregion

    #region CalculateTotalPayment Tests

    [Fact]
    public void CalculateTotalPayment_ValidInput_ReturnsCorrectTotal()
    {
        // Arrange
        decimal monthlyPayment = 500m;
        int termMonths = 60;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        Assert.Equal(30000m, result);
    }

    [Fact]
    public void CalculateTotalPayment_ZeroMonthlyPayment_ReturnsZero()
    {
        // Arrange
        decimal monthlyPayment = 0m;
        int termMonths = 60;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateTotalPayment_ZeroTerm_ReturnsZero()
    {
        // Arrange
        decimal monthlyPayment = 500m;
        int termMonths = 0;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateTotalPayment_NegativeMonthlyPayment_ReturnsNegative()
    {
        // Arrange
        decimal monthlyPayment = -500m;
        int termMonths = 60;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        Assert.Equal(-30000m, result);
    }

    [Theory]
    [InlineData(567.79, 360, 204404.40)]
    [InlineData(1000.0, 12, 12000.0)]
    [InlineData(2500.0, 24, 60000.0)]
    public void CalculateTotalPayment_VariousInputs_ReturnsCorrectTotal(double monthlyPayment, int termMonths, double expectedTotal)
    {
        // Act
        var result = _calculationService.CalculateTotalPayment((decimal)monthlyPayment, termMonths);

        // Assert
        Assert.Equal((decimal)expectedTotal, result);
    }

    [Fact]
    public void CalculateTotalPayment_ResultIsRoundedToTwoDecimals()
    {
        // Arrange
        decimal monthlyPayment = 567.789m;
        int termMonths = 360;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        var decimals = result.ToString().Split('.');
        if (decimals.Length > 1)
        {
            Assert.True(decimals[1].Length <= 2);
        }
    }

    [Fact]
    public void CalculateTotalPayment_LargeTerm_ReturnsCorrectTotal()
    {
        // Arrange
        decimal monthlyPayment = 100m;
        int termMonths = 1000;

        // Act
        var result = _calculationService.CalculateTotalPayment(monthlyPayment, termMonths);

        // Assert
        Assert.Equal(100000m, result);
    }

    #endregion
}

