namespace OC.Assistant.PnGenerator.Aml;

public static class Crc16Arc
{
    public static ushort Calculate(string input)
    {
        const ushort polynomial = 0x8005;
        ushort crc = 0x0000;
        
        foreach (var value in System.Text.Encoding.ASCII.GetBytes(input))
        {
            crc ^= (ushort)(Reverse(value) << 8);
            for (var i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                {
                    crc = (ushort)((crc << 1) ^ polynomial);
                }
                else
                {
                    crc <<= 1;
                }
            }
        }

        return Reverse(crc);
    }

    
    private static byte Reverse(byte value)
    {
        byte result = 0;
        for (var i = 0; i < 8; i++)
        {
            if ((value & (1 << i)) != 0)
            {
                result |= (byte)(1 << (7 - i));
            }
        }
        return result;
    }
    
    private static ushort Reverse(ushort value)
    {
        ushort result = 0;
        for (var i = 0; i < 16; i++)
        {
            if ((value & (1 << i)) != 0)
            {
                result |= (ushort)(1 << (15 - i));
            }
        }
        return result;
    }
}