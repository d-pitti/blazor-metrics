# .Net Blazor PC Metrics hosted in Proxmox VM for local network only

# Description:
Monitor PC metrics from anywhere on a local network (Only Local Network) from a Proxmox Debian 13 VM with .Net Blazor Web App, Nginx as reverse proxy, and Worker Service on PC being monitored. The Idea was to be able to view my work computer metrics on another display, which is a Raspberry Pi 5 with custom 3D print mount on a display with stand, and continue to use my current two monitors and not have to flip back and forth apps.  

# Blazor App:

Using Radzen Blazor library, made a simple gauge display that reads data from the monitored pc through SignalR in C#. The drop-down displays the list of available gauges that are preset from the Worker Service. Can add individual gauges or all the gauges at once. Gauge data is updated every 5 seconds. 

*Gauges Displayed* 

![Blazor App Gauge Configuration](images/pc%20monitor%20screenshot%20.png)

*All Gauges Displayed*

![All Gauges displayed](images/pc%20monitor%20screenshot%20all%20gauges.png)

# PC Worker Service

The Worker Service is in .Net Core and targeted as Web SDK (for signalR communication). The Libre Hardware Monitor Library was used to get information from the PC and add to a list. Then Send the list as a model to the Hub in the blazor app. the Worker Service is published to my Windows 11 PC and runs continuously in the background. Which also has an auto restart if the computer shuts down or network goes offline. 

# Proxmox VM 

This took some time to figure out, since I'm fairly new to SignalR but not new to MQTT and WebSocket's. I developed the folders and published the Blazor app on the Windows PC the SCP in command line to transfer the published Linux targeted folder to my Debian 13 VM. Once there was an established folder to work with, then was able to create the service for the Blazor app running on http localhost. Then tested the app in the VM browser to make sure it was up and running. Next, installed nginx, created a configure file in the sites-available, symlinked the folder to sites-enabled. Ran the app service and then checked in a browser on my Raspberry Pi 5 display, viola, it finally worked. 

# Objective:
I can now watch my PC metrics while I play video games, design 3D CAD models, develop software, or 3D print. Honestly, I love data analysis, so being able to watch what my computer is doing while I stress it out to the max is a nice feature. 

# To-Do:

• Add Color range for storage data.  
• Adjust for media size on small devices.   
• Potentially add more PC metrics features; Fan RPM, Frequencies, etc.  

