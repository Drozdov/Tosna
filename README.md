# What is Tosna?

Tosna provides a convenient way of loading your domain object environment from XML documents on disk.

### Distributed layout
Define your objects in different XML files on your file system. You may reference types from another XML.

### Errors highlighting and autocompletion
Every missing/wrongly spelled field or invalid type will be shown as error (sometimes with a proposed solution). Start writing your type and autocompleting service will give you a hint how to create a new object or reference an existing one.

### Create new configurations programmatically
You may write your own configuration generator that creates default configuration and writes it to files defined by you.

### Simple GUI for some types
If a user is not skilled enough to edit XMLs, some visual controls might help to include, exclude or select existing items in configuration without manual XML editing.

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
