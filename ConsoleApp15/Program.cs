using System;
using System.Collections.Generic;

public static class ValidationHelper
{
    public static bool ValidateSerialNumber(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            return false;

        return serialNumber.Contains("-");
    }
}

public enum RepairStatus
{
    Pending,
    InProgress,
    Completed
}

public delegate void RepairStatusChangedHandler(string serialNumber, RepairStatus newStatus);

public interface IRepairable
{
    string SerialNumber { get; }
    RepairStatus Status { get; }

    decimal CalculateRepairCost();
    void ChangeStatus(RepairStatus newStatus);
    string GetSummary();
}

public abstract class Appliance : IRepairable
{
    private readonly decimal _partsCost;
    private readonly double _laborHours;
    private readonly decimal _laborRatePerHour;
    private RepairStatus _status;

    
    public static event RepairStatusChangedHandler OnStatusChanged;

    public string SerialNumber { get; }
    public RepairStatus Status { get { return _status; } }
    public abstract string ApplianceType { get; }

    protected Appliance(string serialNumber, decimal partsCost,
                        double laborHours, decimal laborRatePerHour)
    {
       
        if (!ValidationHelper.ValidateSerialNumber(serialNumber))
        {
            throw new ArgumentException("الرقم التسلسلي غير صالح!");
        }

        SerialNumber = serialNumber;
        _partsCost = partsCost;
        _laborHours = laborHours;
        _laborRatePerHour = laborRatePerHour;
        _status = RepairStatus.Pending;
    }

    public abstract decimal CalculateRepairCost();

    protected decimal CalculateBaseCost()
    {
        return _partsCost + (decimal)_laborHours * _laborRatePerHour;
    }

    public void ChangeStatus(RepairStatus newStatus)
    {
        _status = newStatus;

        if (OnStatusChanged != null)
        {
            OnStatusChanged(SerialNumber, _status);
        }
    }

    public string GetSummary()
    {
        return string.Format("Type: {0}, Serial: {1}, Status: {2}",
            ApplianceType, SerialNumber, Status);
    }
}

public class Refrigerator : Appliance
{
    private readonly bool _hasFreezer;

    public override string ApplianceType { get { return "Refrigerator"; } }
    public bool HasFreezer { get { return _hasFreezer; } }

    public Refrigerator(string serialNumber, decimal partsCost,
                        double laborHours, decimal laborRatePerHour, bool hasFreezer)
        : base(serialNumber, partsCost, laborHours, laborRatePerHour)
    {
        _hasFreezer = hasFreezer;
    }

    public override decimal CalculateRepairCost()
    {
        decimal baseCost = CalculateBaseCost();
        if (_hasFreezer)
        {
            baseCost += 50.0m;
        }
        return baseCost;
    }
}

public class WashingMachine : Appliance
{
    private readonly int _capacityKg;

    public override string ApplianceType { get { return "WashingMachine"; } }
    public int CapacityKg { get { return _capacityKg; } }

    public WashingMachine(string serialNumber, decimal partsCost,
                          double laborHours, decimal laborRatePerHour, int capacityKg)
        : base(serialNumber, partsCost, laborHours, laborRatePerHour)
    {
        _capacityKg = capacityKg;
    }

    public override decimal CalculateRepairCost()
    {
        decimal baseCost = CalculateBaseCost();
        if (_capacityKg > 8)
        {
            baseCost += 30.0m;
        }
        return baseCost;
    }
}

public class AirConditioner : Appliance
{
    private readonly double _btu;

    public override string ApplianceType { get { return "AirConditioner"; } }
    public double Btu { get { return _btu; } }
    public AirConditioner(string serialNumber, decimal partsCost,
                          double laborHours, decimal laborRatePerHour, double btu)
        : base(serialNumber, partsCost, laborHours, laborRatePerHour)
    {
        _btu = btu;
    }

    public override decimal CalculateRepairCost()
    {
        decimal baseCost = CalculateBaseCost();
        return baseCost + 40.0m;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
       
        Appliance.OnStatusChanged += (serial, status) =>
        {
            Console.WriteLine($"[تنبيه الحدث]: الجهاز ({serial}) تغيرت حالته إلى: {status}");
        };

        List<IRepairable> workshopAppliances = new List<IRepairable>();

        workshopAppliances.Add(new Refrigerator("REF-101", 120.0m, 2.5, 30.0m, true));
        workshopAppliances.Add(new WashingMachine("WM-202", 80.0m, 1.5, 25.0m, 9));
        workshopAppliances.Add(new AirConditioner("AC-303", 200.0m, 3.0, 35.0m, 18000));

        foreach (IRepairable item in workshopAppliances)
        {
            Console.WriteLine(item.GetSummary());
            Console.WriteLine("Repair Cost: " + item.CalculateRepairCost());

            item.ChangeStatus(RepairStatus.InProgress);
            item.ChangeStatus(RepairStatus.Completed);
            Console.WriteLine("-------------------");
        }
    }
}