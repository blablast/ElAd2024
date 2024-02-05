using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;
using ElAd2024.Contracts.Services;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ElAd2024.Services;
public partial class RobotService(string IPAddress) : ObservableRecipient, IRobotService
{
    #region Fields
    private string connectionResult = string.Empty;
    private readonly int pingAttempts = 20;
    private readonly int pingTimeout = 250;

    #endregion

    #region Properties
    public async Task<bool> IsConnectedAsync()
    {
        return await PingAsync() || await ConnectAsync();
    }
    [ObservableProperty] private bool isConnected = false;
    public string IPAddress { get; set; } = IPAddress;
    public List<string> FanucVisions { get; set; } = [];
    public string RobotVistionURL() => $"http://{IPAddress}/frh/vision/vrfrmn.stm";

    #endregion
    #region Constructors

    public async Task InitializeAsync()
    {
        Debug.WriteLine($"RobotService initialized. Connecting status: {IsConnected}.");
        await Task.CompletedTask;
    }
    #endregion

    #region Public Methods

    public void GetFanucVisions() => FanucVisions = FanucData<string>.GetVisions(IPAddress);


    public async Task<bool> ConnectAsync()
    {
        IsConnected = await PingAsync();
        AppNotificationManager.Default.Show(
            new AppNotificationBuilder()
            .AddText(await PingAsync() ? $"Robot Online!" : $"Robot offline!")
            .AddText(connectionResult)
            .BuildNotification()
        );
        GetFanucVisions();
        return IsConnected;
    }


    public bool SetRegister(int index, bool value)
        => SetValue($"http://{IPAddress}:3080/COMET/rpc?func=IOVALSET&type=35&index={index}&value={(value ? 1 : 0)}").GetAwaiter().GetResult();
    //http://{IPAddress}:3080/COMET/rpc?func=IOVALRD&type=35&index=7
    public bool SetRegister(int index, int value)
        => SetValue($"http://{IPAddress}/karel/ComSet?sValue={value}&sIndx={index}&sRealFlag=0&sFc=2").GetAwaiter().GetResult();
    public bool SetRegister(int index, double value)
        => SetValue($"http://{IPAddress}/karel/ComSet?sValue={value.ToString("F6", CultureInfo.InvariantCulture)}&sIndx={index}&sRealFlag=1&sFc=2").GetAwaiter().GetResult();

    public bool SetRegister(int index, string value)
        => SetValue($"http://{IPAddress}/karel/ComSet?sValue={value}&sIndx={index}&sFc=15").GetAwaiter().GetResult();

    public bool GetFlagRegister(int index)
    {
        IsIndexInRange(index, 1, 200);
        var response = GetValue<ResponseRPC>($"http://{IPAddress}:3080/COMET/rpc?func=IOVALRD&type=35&index={index}").GetAwaiter().GetResult()?.FANUC.RPC[0].Value;
        return response is not null && response == "1";
    }

    public (int?, double?) GetNumericRegister(int index)
    {
        IsIndexInRange(index, 1, 200);
        var response = GetValue<ResponseRPC>($"http://{IPAddress}:3080/COMET/rpc?func=REGVALRD&index={index}").GetAwaiter().GetResult();
        var value = response?.FANUC.RPC[0].Value;

        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue))
        {
            return (intValue, null);
        }
        else if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dblValue))
        {
            return (null, dblValue);
        }
        else
        {
            return (null, null);
        }

    }

    // http://192.168.1.100:3080/COMET/rpc?func=REGVALRD&index=2
    public string GetStringRegister(int index)
    {
        IsIndexInRange(index, 1, 25);
        return FanucData<string>.GetRegisters("STRREG.VA", IPAddress)[index - 1];
    }

    public void ChangeOverride(int value)
        => SetValue($"http://{IPAddress}:3080/COMET/rpc?func=CHGOVRD&ovrd_val={value}").GetAwaiter().GetResult();

    public PositionXYZWPR CurrentPosition()
        => (new PositionXYZWPR()).Decode(GetValue<ResponseRPC>($"http://{IPAddress}/COMET/rpc?func=TXML_CURPOS&pos_rep=1&pos_type=1&grp_num=1").GetAwaiter().GetResult()?.FANUC.RPC[0].Value) ?? new PositionXYZWPR();

    // http://ROBOTIP/KCL/set%20port%20DOUT[1]=1 SET DO0 to 0

    #endregion

    #region private Methods

    private static bool IsIndexInRange(int index, int min, int max) => index < min || index > max ? throw new ArgumentOutOfRangeException(nameof(index)) : true;

    private static readonly JsonSerializerOptions GetValueJsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
private static async Task<Response<T>?> GetValue<T>(string url)
    {
        try
        {
            var response = await new HttpClient().GetAsync(url).ConfigureAwait(false);
            return response.IsSuccessStatusCode ?
                JsonSerializer.Deserialize<Response<T>>(await response.Content.ReadAsStringAsync(), GetValueJsonSerializerOptions) :
                null;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<bool> SetValue(string url)
    {
        try
        {
            return (await new HttpClient().GetAsync(url).ConfigureAwait(false)).IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    private async Task<bool> PingAsync()
    {
        var ping = new Ping();
        PingReply? reply;
        var counter = 0;
        do
        {
            try
            {
                reply = await ping.SendPingAsync($"{IPAddress}", pingTimeout);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            var response = new StringBuilder();
            if (reply.Status == IPStatus.Success)
            {
                response.AppendLine($"Address: {reply.Address}");
                response.AppendLine($"RoundTrip time: {reply.RoundtripTime} ms");
                response.AppendLine($"Time to live: {reply.Options?.Ttl} ms");
                response.AppendLine($"Succesfull attempt: {counter + 1}");
            }
            else
            {
                response.AppendLine(reply.Status.ToString());
                response.AppendLine($"Attempts: {counter + 1}");
                await Task.Delay(1000);
            }
            connectionResult = response.ToString();
        } while (++counter < pingAttempts && reply.Status != IPStatus.Success);
        return reply.Status == IPStatus.Success;
    }

}


internal class FanucData<T>
{
    public static List<string> GetVisions(string IPAddress)
    {
        List<string> registers = [];
        try
        {
            var responseBody = new HttpClient().GetStringAsync($"http://{IPAddress}/MD/INDEX_VD.HTM").GetAwaiter().GetResult();
            HtmlDocument htmlSnippet = new();
            htmlSnippet.LoadHtml(responseBody);
            var words = htmlSnippet.DocumentNode?.SelectNodes($"//table")[2]?.InnerText.Split('\n') ?? [];
            for (var i = 0; i < words.Length; i++)
            {
                if (words[i].Contains("Vision VD file"))
                {
                    registers.Add(words[i].Replace("Vision VD file", string.Empty).Trim().Replace(".VD", string.Empty));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return registers;
    }

    public static List<T> GetRegisters(string fanucFileName, string IPAddress)
    {
        HttpClient client = new();
        List<T> registers = [];
        try
        {
            var responseBody = client.GetStringAsync($"http://{IPAddress}/MD/{fanucFileName}").GetAwaiter().GetResult();
            var words = FanucData<T>.FanucRegHtmlParse(responseBody).Split('\n');

            for (var i = 2; i < words.Length - 4; i++)
            {
                var text = words[i][(words[i].IndexOf('=') + 2)..words[i].IndexOf('\'')].TrimEnd();
                if (typeof(T) == typeof(double) && double.TryParse(text, out var value))
                {
                    registers.Add((T)Convert.ChangeType(value, typeof(T)));
                }
                else if (typeof(T) == typeof(string))
                {
                    registers.Add((T)Convert.ChangeType(text, typeof(T)));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return registers;
    }

    private static string FanucRegHtmlParse(string Html)
    {
        HtmlDocument htmlSnippet = new();
        htmlSnippet.LoadHtml(Html);
        try
        {
            var pre = htmlSnippet.DocumentNode?.SelectNodes($"//body/pre")[0];
            return pre?.InnerText.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
    #endregion
}

internal class FanucRegister
{
    public enum RobotRegisterTypes
    {
        SR, R, F
    }
    public int RegNumber
    {
        get; set;
    }
    public RobotRegisterTypes RegisterType
    {
        get; set;
    }
    public object? Value
    {
        get; set;
    }
}

internal class Response<T>
{
    public ResponseFANUC<T> FANUC { get; set; } = new();
}

internal class ResponseFANUC<T>
{
    public string Name { get; set; } = string.Empty;
    public string Fastclock { get; set; } = string.Empty;
    public List<T> RPC { get; set; } = [];
}

internal class ResponseRPC
{
    public string Rpc { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

internal class ResponseRPCFlag : ResponseRPC
{
    public string Type { get; set; } = string.Empty;
    public string Index { get; set; } = string.Empty;
}

public class PositionXYZWPR
{
    public PositionXYZWPR? Decode(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        // Define the regular expression pattern
        var pattern = @"X:\s*([\d.-]+)\s*Y:\s*([\d.-]+)\s*Z:\s*([\d.-]+)\s*W:\s*([\d.-]+)\s*P:\s*([\d.-]+)\s*R:\s*([\d.-]+)";

        // Match the pattern in the value string
        var match = Regex.Match(value, pattern);

        if (match.Success)
        {
            // Using double.Parse
            X = ParseAndRound(match.Groups[1].Value);
            Y = ParseAndRound(match.Groups[2].Value);
            Z = ParseAndRound(match.Groups[3].Value);
            W = ParseAndRound(match.Groups[4].Value);
            P = ParseAndRound(match.Groups[5].Value);
            R = ParseAndRound(match.Groups[6].Value);
        }
        else
        {
            // Set null values
            X = null; Y = null; Z = null; W = null; P = null; R = null;
        }
        return match.Success ? this : null;
    }
    public void Deconstruct(out double? x, out double? y, out double? z, out double? w, out double? p, out double? r)
    {
        x = X;
        y = Y;
        z = Z;
        w = W;
        p = P;
        r = R;
    }

    private static double? ParseAndRound(string value)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue))
        {
            return Math.Round(parsedValue, 2);
        }
        return null;
    }

    public double? X
    {
        get; set;
    }
    public double? Y
    {
        get; set;
    }
    public double? Z
    {
        get; set;
    }
    public double? W
    {
        get; set;
    }
    public double? P
    {
        get; set;
    }
    public double? R
    {
        get; set;
    }
    public bool IsValid => this is not null && X is not null && Y is not null && Z is not null && W is not null && P is not null && R is not null;

    public override string ToString()
    {
        return $"X: {X} Y: {Y} Z: {Z} W: {W} P: {P} R: {R}";
    }
}