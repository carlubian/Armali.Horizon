# Armali.Horizon.Autoconfig

This project is a library for automatically configuring Horizon applications.  It provides a simple and efficient 
way to set up your Horizon application without having to manually configure each component.

## Features

- Automatic provisioning of appsettings to other components and applications.
- Seamless integration with Horizon applications.
- UI to manage and view the configuration of your Horizon application.

## Structure

The configuration hierarchy is as follows:

### Node

Nodes represent physical (or virtual) devices.
Apps are then deployed on a node, each with its own set of configurations. 
This allows you to manage the configurations of your applications based on the nodes they are deployed on.

### App

This level represents each of the applications in your Horizon environment. 
Each app can have its own set of configurations.

### Version

This level represents the different versions of each application.
Each version can have its own set of configurations, allowing you to manage multiple versions of your application with ease.

### File

This level represents the individual configuration files for each version of your application.
Normally this will contain an "appsettings.json" file, but it can be extended to include other configuration files as needed.