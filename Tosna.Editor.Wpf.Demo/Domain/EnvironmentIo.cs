using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tosna.Core.Common;
using Tosna.Core.Common.Stamps;
using Tosna.Core.IO;
using Tosna.Core.SerializationInterfaces;
using Tosna.Editor.Wpf.Demo.Domain.Common;
using Tosna.Editor.Wpf.Demo.Domain.Devices;

namespace Tosna.Editor.Wpf.Demo.Domain;

public static class EnvironmentIo
{
	public static void CreateAndSaveDefaultEnvironment(string initialFile)
	{
		var entryPointFilePath = new FileInfo(initialFile);
		var entryPointDirectory = entryPointFilePath.Directory;
		
		var device1 = new DummyDeviceSignature();
		var device2 = new DummyDeviceSignature();
		var device3 = new DummyDeviceSignature();

		var yerevanPosition = new GeographicalCoordinates(40.177633, 44.512545, 0);
		var stPetersburgPosition = new GeographicalCoordinates(59.939014, 30.315724, 0);
		var berlinPosition = new GeographicalCoordinates(52.518368, 13.374546, 0);
		var potsdamPosition = new GeographicalCoordinates(52.399420, 13.047081, 0);
		
		var modBusAnemometer = new ModBusNetworkAnemometerSignature("192.168.32.13", 5566, yerevanPosition);

		var yerevanThermometer = new ConstTemperatureThermometerSignature(30f, 0.1f, yerevanPosition);
		var potsdamThermometer = new ConstTemperatureThermometerSignature(20f, 0.5f, potsdamPosition);
		var randomizedBarometer = new RandomizedBarometerSignature();
		
		var yerevanStation = new WeatherStationSignature(
			new IDeviceSignature<IThermometer>[] { device1, yerevanThermometer },
			new IDeviceSignature<IBarometer>[] { device1, randomizedBarometer },
			new IDeviceSignature<IAnemometer>[] { device1, modBusAnemometer },
			yerevanPosition,
			"Yerevan");

		var stPetersburgStation = new WeatherStationSignature(
			new IDeviceSignature<IThermometer>[] { device2 },
			new IDeviceSignature<IBarometer>[] { device2 },
			new IDeviceSignature<IAnemometer>[] { device2 },
			stPetersburgPosition,
			"St. Petersburg");

		var berlinStation = new WeatherStationSignature(
			new IDeviceSignature<IThermometer>[] { device2, device3, potsdamThermometer },
			new IDeviceSignature<IBarometer>[] {  device2, device3, randomizedBarometer },
			new IDeviceSignature<IAnemometer>[] {  device2, device3 },
			berlinPosition,
			"Berlin");

		var environment = new WeatherStationsEnvironmentSignature(
			new IDeviceSignature<WeatherStation>[] { yerevanStation, stPetersburgStation, berlinStation },
			"Environment1");

		var serializingElementsManager = new SerializingElementsManager();
		var signaturesSerializingTypeResolver = new SignaturesSerializingTypeResolver(serializingElementsManager);
		var constructor =
			new StampsCollectionConstructor(serializingElementsManager, signaturesSerializingTypeResolver);

		constructor.AddObject(environment);

		constructor.GetStamp(environment).UserId = "Environment1";

		FileInfo positionsFilePath;
		FileInfo devicesFilePath;
		FileInfo yerevanFilePath;
		FileInfo stPetersburgFilePath;
		FileInfo berlinFilePath;
		
		if (entryPointDirectory != null)
		{
			positionsFilePath = new FileInfo(Path.Combine(entryPointDirectory.FullName, "Details", "Positions.xml"));
			devicesFilePath = new FileInfo(Path.Combine(entryPointDirectory.FullName, "Details", "Devices.xml"));
			yerevanFilePath = new FileInfo(Path.Combine(entryPointDirectory.FullName, "Stations", "Yerevan.xml"));
			stPetersburgFilePath = new FileInfo(Path.Combine(entryPointDirectory.FullName, "Stations", "StPetersburg.xml"));
			berlinFilePath = new FileInfo(Path.Combine(entryPointDirectory.FullName, "Stations", "Berlin.xml"));
		}
		else
		{
			positionsFilePath = new FileInfo(Path.Combine("Details", "Positions.xml"));
			devicesFilePath = new FileInfo(Path.Combine("Details", "Devices.xml"));
			yerevanFilePath = new FileInfo(Path.Combine("Stations", "Yerevan.xml"));
			stPetersburgFilePath = new FileInfo(Path.Combine("Stations", "StPetersburg.xml"));
			berlinFilePath = new FileInfo(Path.Combine("Stations", "Berlin.xml"));
		}
		
		constructor.GetStamp(yerevanPosition).UserId = "Yerevan";
		constructor.GetStamp(yerevanPosition).UserFilePath = positionsFilePath;

		constructor.GetStamp(stPetersburgPosition).UserId = "StPetersburg";
		constructor.GetStamp(stPetersburgPosition).UserFilePath = positionsFilePath;
		
		constructor.GetStamp(berlinPosition).UserId = "Berlin";
		constructor.GetStamp(berlinPosition).UserFilePath = positionsFilePath;
		
		constructor.GetStamp(potsdamPosition).UserId = "Potsdam";
		constructor.GetStamp(potsdamPosition).UserFilePath = positionsFilePath;

		constructor.GetStamp(device1).UserId = "Device1";
		constructor.GetStamp(device1).UserFilePath = devicesFilePath;
		
		constructor.GetStamp(device2).UserId = "Device2";
		constructor.GetStamp(device2).UserFilePath = devicesFilePath;
		
		constructor.GetStamp(device3).UserId = "Device3";
		constructor.GetStamp(device3).UserFilePath = devicesFilePath;
		
		constructor.GetStamp(modBusAnemometer).UserId = "ModBusAnemometer";
		constructor.GetStamp(modBusAnemometer).UserFilePath = devicesFilePath;
		
		constructor.GetStamp(yerevanThermometer).UserId = "YerevanThermometer";
		constructor.GetStamp(yerevanThermometer).UserFilePath = devicesFilePath;
		
		constructor.GetStamp(potsdamThermometer).UserId = "PotsdamThermometer";
		constructor.GetStamp(potsdamThermometer).UserFilePath = devicesFilePath;
		
		constructor.GetStamp(randomizedBarometer).UserId = "Barometer";
		constructor.GetStamp(randomizedBarometer).UserFilePath = devicesFilePath;
		
		constructor.GetStamp(yerevanStation).UserId = "WeatherStation";
		constructor.GetStamp(yerevanStation).UserFilePath = yerevanFilePath;

		constructor.GetStamp(stPetersburgStation).UserId = "WeatherStation";
		constructor.GetStamp(stPetersburgStation).UserFilePath = stPetersburgFilePath;
		
		constructor.GetStamp(berlinStation).UserId = "WeatherStation";
		constructor.GetStamp(berlinStation).UserFilePath = berlinFilePath;
		
		var stampsEnvironment = new StampsEnvironment(constructor.GetCollection(), entryPointFilePath);
		StampsEnvironmentWriter.Save(stampsEnvironment, new ImprintsSerializer(serializingElementsManager, signaturesSerializingTypeResolver));
	}

	public static bool TryReadEnvironment(IEnumerable<string> files, out WeatherStationsEnvironment environment)
	{
		var serializingElementsManager = new SerializingElementsManager();
		var signaturesSerializingTypeResolver = new SignaturesSerializingTypeResolver(serializingElementsManager);
		var imprintsSerializer = new ImprintsSerializer(serializingElementsManager, signaturesSerializingTypeResolver);

		var imprints = ImprintsEnvironmentReader.Read(imprintsSerializer, files.ToArray());
		var objects= imprints.GetAll();
		var environmentSignature = objects.OfType<WeatherStationsEnvironmentSignature>().FirstOrDefault();
		if (environmentSignature == null)
		{
			environment = default(WeatherStationsEnvironment);
			return false;
		}

		var resolver = new DeviceSignatureResolver();
		environment = resolver.GetDevice(environmentSignature);
		return true;
	}
}