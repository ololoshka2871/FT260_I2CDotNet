using System;

namespace FT260_I2CDotNet
{
    public enum PinID
    {
        DIO0_GPIOA,
        DIO1_GPIOB,
        DIO2_GPIOE,
        DIO7_GPIO2,
        DIO8_GPIO3,
        DIO9_GPIOF,
        DIO10_GPIO4,
        DIO11_GPIO5,
        DIO12_GPIOG,
        DIO13_GPIOH,
    }

    public static class PinIDExt
    {
        public static byte ToMask(this PinID id)
        {
            switch (id)
            {
                case PinID.DIO7_GPIO2: return 1 << 2;
                case PinID.DIO8_GPIO3: return 1 << 3;
                case PinID.DIO10_GPIO4: return 1 << 4;
                case PinID.DIO11_GPIO5: return 1 << 5;
                case PinID.DIO0_GPIOA: return 1 << 0;
                case PinID.DIO1_GPIOB: return 1 << 1;
                case PinID.DIO2_GPIOE: return 1 << 4;
                case PinID.DIO9_GPIOF: return 1 << 5;
                case PinID.DIO12_GPIOG: return 1 << 6;
                case PinID.DIO13_GPIOH: return 1 << 7;
                default: return 0;
            }
        }

        public static bool IsExtension(this PinID id)
        {
            switch (id)
            {
                case PinID.DIO7_GPIO2: 
                case PinID.DIO8_GPIO3: 
                case PinID.DIO10_GPIO4:
                case PinID.DIO11_GPIO5:
                    return false;
                default:
                    return true;
            }
        }
    }

    public enum PinState
    {
        DEFAULT,
        INPUT,
        OUTPUT,
    }

    public class GPIOState
    {
        public PinID Id;
        public PinState State;
    }

    public class GPIOReadState: GPIOState
    {
        public bool Value;
    }

    public class GPIOWriteState: GPIOState
    {
        public bool Value = false;
    }
}
