using System;
using System.Globalization;

class Program
{
    // Текущие значение и память
    static double current = 0.0;
    static double memory = 0.0;

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Калькулятор (C#). Команды: число | + n | - n | * n | / n | % n | inv | sq | sqrt | M+ | M- | MR | C | AC | help | exit");
        Console.WriteLine("Подсказка: введите число для установки текущего значения, затем операции. Примеры: '+ 5', '* 2', 'sqrt', 'M+', 'MR'.");

        PrintState();

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null) break;

            line = line.Trim();
            if (line.Length == 0) continue;

            // Команды выхода
            if (line.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (line.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                PrintHelp();
                continue;
            }

            // Попробуем интерпретировать ввод как число (установка текущего значения)
            if (TryParseNumber(line, out double numOnly))
            {
                current = numOnly;
                PrintState();
                continue;
            }

            // Разбор операций формата: "op [number]"
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var op = parts[0].ToLowerInvariant();
            double arg = double.NaN;

            if (parts.Length > 1)
            {
                var rest = string.Join(" ", parts, 1, parts.Length - 1);
                if (!TryParseNumber(rest, out arg))
                {
                    Console.WriteLine("Ошибка: не удалось разобрать число аргумента.");
                    continue;
                }
            }

            try
            {
                switch (op)
                {
                    case "+":
                        RequireArg(parts.Length);
                        current = checked(current + arg);
                        break;

                    case "-":
                        RequireArg(parts.Length);
                        current = checked(current - arg);
                        break;

                    case "*":
                    case "x":
                        RequireArg(parts.Length);
                        current = checked(current * arg);
                        break;

                    case "/":
                    case "÷":
                        RequireArg(parts.Length);
                        if (arg == 0) throw new DivideByZeroException("Деление на ноль.");
                        current = current / arg;
                        break;

                    case "%":
                        RequireArg(parts.Length);
                        // Классическая операция остатка (mod) для double
                        if (arg == 0) throw new DivideByZeroException("Остаток по модулю на 0 невозможен.");
                        current = current % arg;
                        break;

                    case "inv":     // 1/x
                    case "1/x":
                        if (current == 0) throw new DivideByZeroException("1/0 невозможно.");
                        current = 1.0 / current;
                        break;

                    case "sq":      // x^2
                    case "x^2":
                        current = checked(current * current);
                        break;

                    case "sqrt":    // √x
                    case "√":
                        if (current < 0) throw new ArgumentOutOfRangeException(nameof(current), "Корень из отрицательного числа не определён в R.");
                        current = Math.Sqrt(current);
                        break;

                    case "m+":      // память плюс текущее
                    case "mplus":
                        memory += current;
                        break;

                    case "m-":      // память минус текущее
                    case "mminus":
                        memory -= current;
                        break;

                    case "mr":      // memory recall
                    case "memory":
                        current = memory;
                        break;

                    case "c":       // сброс текущего
                        current = 0.0;
                        break;

                    case "ac":      // сброс всего
                        current = 0.0;
                        memory = 0.0;
                        break;

                    default:
                        Console.WriteLine("Неизвестная команда. Введите 'help' для справки.");
                        continue;
                }

                PrintState();
            }
            catch (OverflowException)
            {
                Console.WriteLine("Ошибка: переполнение при вычислении (слишком большое число).");
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неожиданная ошибка: {ex.Message}");
            }
        }

        Console.WriteLine("Завершено.");
    }

    static void PrintHelp()
    {
        Console.WriteLine("Команды:");
        Console.WriteLine("  <число>      — установить текущее значение (напр. 12,5 или 12.5)");
        Console.WriteLine("  + n  - n     — сложение / вычитание с n");
        Console.WriteLine("  * n  / n     — умножение / деление на n");
        Console.WriteLine("  % n          — остаток от деления на n");
        Console.WriteLine("  inv (1/x)    — обратить текущее значение");
        Console.WriteLine("  sq (x^2)     — квадрат текущего значения");
        Console.WriteLine("  sqrt         — квадратный корень");
        Console.WriteLine("  M+           — прибавить текущее к памяти");
        Console.WriteLine("  M-           — вычесть текущее из памяти");
        Console.WriteLine("  MR           — записать из памяти в текущее");
        Console.WriteLine("  C            — сбросить текущее (0)");
        Console.WriteLine("  AC           — сбросить текущее и память (0)");
        Console.WriteLine("  help         — показать помощь");
        Console.WriteLine("  exit         — выход");
        Console.WriteLine("Примеры: '25', '+ 5', '* 2', 'sqrt', 'M+', 'MR'.");
    }

    static void PrintState()
    {
        Console.WriteLine($"Текущее: {Format(current)}    Память: {Format(memory)}");
    }

    static void RequireArg(int partsLen)
    {
        if (partsLen < 2)
            throw new ArgumentException("Для этой операции требуется аргумент (число).");
    }

    static bool TryParseNumber(string s, out double value)
    {
        // Принимаем как запятую, так и точку (ru/en)
        s = s.Trim();
        // Сначала пробуем текущую культуру
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            return true;
        // Потом Invariant (точка)
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;
        // И наконец заменим запятую на точку явно
        var normalized = s.Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    static string Format(double v)
    {
        // Короткий формат без лишних нулей
        return v.ToString("G10", CultureInfo.CurrentCulture);
    }
}
