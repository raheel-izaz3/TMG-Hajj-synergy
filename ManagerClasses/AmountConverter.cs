using TMG.Models;

namespace TMG.ManagerClasses;

public static class AmountConverter
{
    public static float GetConvertedAmount(float amount, Currencies baseCurrency, Currencies convertTo, float roe)
    {
        if (baseCurrency == convertTo) return amount;

        float convertedAmount = amount;

        if (baseCurrency == Currencies.PKR)
        {
            if (convertTo == Currencies.USD)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.SAR)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.GBP)
            {
                convertedAmount = amount / roe;
            }
            else if (convertTo == Currencies.AED)
            {
                convertedAmount = amount / roe;
            }
        }
        else if (baseCurrency == Currencies.USD)
        {
            if (convertTo == Currencies.PKR)
            {
                convertedAmount = amount / roe;
            }
            else if (convertTo == Currencies.SAR)
            {
                convertedAmount = amount / roe;
            }
            else if (convertTo == Currencies.GBP)
            {
                convertedAmount = amount / roe;
            }
            else if (convertTo == Currencies.AED)
            {
                convertedAmount = amount / roe;
            }
        }
        else if (baseCurrency == Currencies.SAR)
        {
            if (convertTo == Currencies.PKR)
            {
                convertedAmount = amount / roe;
            }
            else if (convertTo == Currencies.USD)
            {
                convertedAmount = amount / roe;
            }
            else if (convertTo == Currencies.GBP)
            {
                convertedAmount = amount / roe;
            }
            else if (convertTo == Currencies.AED)
            {
                convertedAmount = amount / roe;
            }
        }
        else if (baseCurrency == Currencies.GBP)
        {
            if (convertTo == Currencies.PKR)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.USD)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.SAR)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.EURO)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.AED)
            {
                convertedAmount = amount / roe;
            }
        }
        else if (baseCurrency == Currencies.EURO)
        {
            if (convertTo == Currencies.PKR)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.USD)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.SAR)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.GBP)
            {
                convertedAmount = amount / roe;
            }
            else if (convertTo == Currencies.AED)
            {
                convertedAmount = amount / roe;
            }
        }
        else if (baseCurrency == Currencies.AED)
        {
            if (convertTo == Currencies.PKR)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.USD)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.SAR)
            {
                convertedAmount = amount * roe;
            }
            else if (convertTo == Currencies.GBP)
            {
                convertedAmount = amount / roe;
            }
            else if (convertTo == Currencies.EURO)
            {
                convertedAmount = amount / roe;
            }
        }

        return convertedAmount;
    }
}

