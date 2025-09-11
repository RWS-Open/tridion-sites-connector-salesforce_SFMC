Build steps

Required Visual studio 2022

	Salesforce Marketing connector addon 
		Sdl.Connectors.Tridion.SalesforceConnector :	target framework : .NET Standard 2.0
		
		Addons will be generated in \SalesforceAddon.Build\AddonPackage\
			Update the SMFC details accordingly in SalesforceAddonConfiguration.json
		
	tridion-integration-accelerators
		Sdl.Tridion.Api.Client :	target framework :  .NET Framework 4.8
		
		Sdl.Dxa.Integration.Client :	target framework :  .NET Framework 4.6.2
	  
		Sdl.Dxa.Integration.Form :	target framework :  .NET Framework 4.6.2
		 