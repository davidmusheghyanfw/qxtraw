public class PrinterData
{
    public string barCode;
    public string date;
    public int ticketNumber;
    public float balanceLeft;
    public float amount;
    public string currency;
    public int ticketVoid;
    public string machineNumber;
    public string establishment;
    public string validation;

    public int CurrencyAsInt => int.TryParse(currency, out int result) ? result : 0;

    public static string ConvertToWords(float amount, string currency)
    {
        // Split the amount into whole and fractional parts
        int wholePart = (int)Math.Floor(amount);
        int fractionalPart = (int)((amount - wholePart) * 100);

        // Convert the whole and fractional parts to words
        string wholePartWords = NumberToWords(wholePart).ToUpper();
        string fractionalPartWords = fractionalPart > 0 ? $"AND {NumberToWords(fractionalPart).ToUpper()} CENTS" : "";

        // Get the currency name in plural form
        string currencyName = currency + "S";

        // Combine into the final result
        return $"{wholePartWords} {currencyName} {fractionalPartWords}".Trim();
    }
    private static string NumberToWords(int number)
    {
        if (number == 0)
            return "zero";

        if (number < 0)
            return "minus " + NumberToWords(Math.Abs(number));

        string words = "";

        if ((number / 1000) > 0)
        {
            words += NumberToWords(number / 1000) + " thousand ";
            number %= 1000;
        }

        if ((number / 100) > 0)
        {
            words += NumberToWords(number / 100) + " hundred ";
            number %= 100;
        }

        if (number > 0)
        {
            if (!string.IsNullOrEmpty(words))
                words += "and ";

            var unitsMap = new[]
            {
                "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
                "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen",
                "eighteen", "nineteen"
            };
            var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }
        }

        return words.Trim();
    }

    public override string ToString()
    {
        // Format: ^P|0|1|barcode|establishment||||barcode|date|time|ticketNumber|amountInWords|PR G|amount||30 days|machineNumber|validation|^
        string barcodeStr = barCode.ToString();// barCode.ToString("D16").Insert(2, "-").Insert(7, "-").Insert(12, "-");
        string dateStr = !string.IsNullOrEmpty(date) ? date : DateTime.Now.ToString("dd/MM/yyyy");
        string timeStr = DateTime.Now.ToString("HH:mm:ss");
        string ticketStr = $"Ticket # {ticketNumber}";
        string amountInWords = ConvertToWords(amount, currency);
        string amountStr = $"${amount:F2}";
        string machineStr = !string.IsNullOrEmpty(machineNumber) ? machineNumber : "MACHINE#1234-678";
        string establishmentStr = !string.IsNullOrEmpty(establishment) ? establishment : "Your Establishment";
        string validationStr = !string.IsNullOrEmpty(validation) ? validation : barcodeStr.Replace("-", "");
        string locationStr = "";
        string cityStateZipStr = "";

        return $"^P|0|1|{barcodeStr}|{establishmentStr}|{locationStr}|{cityStateZipStr}|||" +
       $"{barcodeStr}|{dateStr}|{timeStr}|{ticketStr}|{amountInWords}|PR G|{amountStr}||30 days|{machineStr}|{validationStr}|^";

    }
}