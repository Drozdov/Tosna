# What is Tosna?

Tosna provides a convenient way of loading your domain object environment from XML documents on disk.

### Distributed layout
Define your objects in different XML files on your file system. You may reference types from another XML.

<img width="895" alt="CommonView" src="https://user-images.githubusercontent.com/3025230/205515839-4f793630-2b34-4cc8-9047-98518bd7ff91.png">

### Errors highlighting and autocompletion
Every missing/wrongly spelled field or invalid type will be shown as error (sometimes with a proposed solution). Start writing your type and autocompleting service will give you a hint how to create a new object or reference an existing one.

<img width="674" alt="EditorHints1" src="https://user-images.githubusercontent.com/3025230/205515840-649f151b-b875-40a9-9cbc-4803bafbe330.png">
<img width="671" alt="EditorHints2" src="https://user-images.githubusercontent.com/3025230/205515841-116781b4-b837-4cb7-9b14-056388336eea.png">

### Create new configurations programmatically
You may write your own configuration generator that creates default configuration and writes it to files defined by you.

### Simple GUI for some types
If a user is not skilled enough to edit XMLs, some visual controls might help to include, exclude or select existing items in configuration without manual XML editing.

<img width="688" alt="Fields configurator" src="https://user-images.githubusercontent.com/3025230/205515843-a5984b80-c762-406a-b38e-01bc644b87cf.png">

### Custom way of objects serialization.
Define your own way of serializing types or use one from demo examples.

# Syntax

Here is a simple example of a simple XML item.

```XML

<?xml version="1.0" encoding="utf-8"?>
<Items> <!-- Items must be the top tag -->
  
  <!-- Defining a new instance of DummyDevice with no fields and identifier for referencing "Device1" -->
  <DummyDevice Global.Id="Device1" />  
  
  <!--
    Defining a new instance of ModBusAnemometer with specified IpAddress, Port and Position.
    Position is passed by reference and is defined in another XML file (Positions.xml)  
  -->
    <ModBusNetworkAnemometer Global.Id="ModBusAnemometer" IpAddress="192.168.0.1" Port="5566">
    <ModBusNetworkAnemometer.Position>
      <GeographicalCoordinates Reference.Id="Position1" Reference.Path="Positions.xml" />
    </ModBusNetworkAnemometer.Position>
  </ModBusNetworkAnemometer>
</Items>

```

For more examples, see Tosna.Wpf.Editor.Demo.

# Getting started

To start working with Tosna, you need to implement these two interfaces:

```C#

// Gets all fields from a type (with a user-frindly name, type, getter, setter and defaults)
// Note that autogenerated backing field names are not user friendly
public interface ISerializingElementsManager
{
	IEnumerable<SerializingElement> GetAllElements(Type type);
}

// Defines the types you are going to work with
public interface ISerializingTypesResolver
{
	IEnumerable<Type> GetAllTypes();
  
	bool TryGetName(Type type, out string name);
  
	bool TryGetType(string name, out Type type);
  
	bool IsSimpleType(Type type);
  
	string SerializeSimple(object item);
  
	object DeserializeSimple(string value, Type type);
}

```

Simple types here are types that can be easily serilized to a string value (and deserialized from one). Primitive types and strings _must_ be in this list.

This is strictly recommended that you are going to serialize items with no behaviour (like "DTO" or "Memo" objects). If you need to create a complicated object with states, events, etc, just serialize parameters needed for your object construction.

When you are done with interfaces required, just do the following:

```C#
var imprintsSerializer = new ImprintsSerializer(serializingElementsManager, signaturesSerializingTypeResolver);
var imprints = ImprintsEnvironmentReader.Read(imprintsSerializer, new[] {"MyFile.xml"});
var myObject = imprints.Get(new ImprintIdentifier("MyObjectId", "MyFile.xml"));
```

This code reads object with identifier MyObjectId from file "MyFile.xml". You may generate this xml from code using StampsCollectionConstructor or manually using Tosna XML editor.
