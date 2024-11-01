namespace Telegram.Bot.YouTuber.Webhook.Extensions;

public static class BytesToHumanExtensions
{
    // https://gist.github.com/BrunoVT1992/b17948089406fdb6cab2b5e9f9d9a9e6

    public static string BytesToHuman(this long size)
    {
        const long kb = 1 * 1024;
        const long mb = kb * 1024;
        const long gb = mb * 1024;
        const long tb = gb * 1024;
        const long pb = tb * 1024;
        const long eb = pb * 1024;

        if (size == 0)
            return "0b";
        
        if (size < kb)
            return FloatForm(size) + "b";

        if (size < mb)
            return FloatForm((double)size / kb) + "Kb";

        if (size < gb)
            return FloatForm((double)size / mb) + "Mb";

        if (size < tb)
            return FloatForm((double)size / gb) + "Gb";

        if (size < pb)
            return FloatForm((double)size / tb) + "Tb";

        if (size < eb)
            return FloatForm((double)size / pb) + "Pb";

        return FloatForm((double)size / eb) + "Eb";
    }

    private static string FloatForm(double d)
    {
        return Math.Round(d, 2).ToString("#.00", System.Globalization.CultureInfo.InvariantCulture);
    }
}