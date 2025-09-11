## Tridion Content Manager Configuragtion Steps

1. Download the [SFMC_Deployer.zip](https://github.com/RWS-Open/tridion-sites-connector-salesforce_SFMC/releases)

2. Create a new publication in Tridion Content manager with following values ( example below , change as per your needs)
   - Name			 : 750 Salesforce Marketing Cloud (en-US)
   - Publication Key  : 750 Salesforce Marketing Cloud (en-US)
   - Type 			 :web
   - publication path : \sfmc
   - publication url  : /sfmc/
   - image path 		 : \sfmc\media
   - image url 		 : /sfmc/media/
   - language  		 : english
   - region 			 : United States (Us)
   - Medataschema 	 : none
   - Blue Print Parent: 300 english

3. Add TtmMapping for 750 publication , Open powershell as administrator  and run the below commands (Mapping id is based on your number of mappings done earlier)

	Add-TtmMapping -Id Mapping14 -CmEnvironmentId Tridioncm_TRIDIONDEMO -PublicationID tcm:0-42-1 -WebApplicationId  TridionDemoWebsite_RootWebApp -RelativeUrl '/sfmc'

4. Upload the addons  and restart the deployer and content service (addons available in SFMC_Deployer.zip  /SFMC_Addons folder)
   - multi-channel-delivery-deployer-extension-1.0.0-tcf-slim
   - SalesforceMarketingCloudAddon-1.0.0
   - udp-content-connector-framework-extension-assembly-12.2.0-1061-core.zip (available under the Tridion sites software release zip)
   - SFMCExampleTemplates-1.0.0.zip

5. Import the packages  (available in SFMC_Deployer.zip file /ImportPackages folder)
   
		a. In the same sequence   (00__****   till   07__****  ) 
		
		b. Update the IntegrationForm MetadataDesign fileds (100->Building Blocks -> Modules -> IntegrationForm - > Editor-> Schema IntegrationForm) as per the Screenshots attached (for reference 09__a_IntergrationForm_MetadataDesign_Screen_formType and soon )
		
		c. Update the Schemas in 100 publicaiton Add the SFMC ECL stub schema (e.g. ExternalContentLibraryStubSchema-sfmc) to 'Allowed Schemas' on the following schema fields:
		   - 'externalField' in the schema 'Building Blocks\Modules\IntegrationForm\Editor\Schemas\Form Field'
		   - 'externalField' in the schema 'Building Blocks\Modules\IntegrationForm\Editor\Schemas\Static Form Field'
		 
		d. Create a componet as per 08__Note.txt (txt file available under importpackage folder)
		
		e. Import the pending packages (09 and 10) 

6. Copy IntegrationForm and OneDemo folder to C:\tridion\Websites\DXA_Preview_org\DXA_Preview\Areas\ 

7. Copy dll  from SFMC_Deployer \DXAFiles\bin folder to c:\tridion\website\DXA_Preview\Bin\
   - Tridion.ConnectorFramework.Connector.SDK.dll
   - Tridion.ConnectorFramework.Contracts.dll
   - Tridion.Remoting.Contracts.dll
   - Sdl.Tridion.Api.Client.dll
   - Sdl.Dxa.Integration.Form.dll
   - Sdl.Dxa.Integration.Client.dll		
   - Newtonsoft.Json.dll

8. Updated c:\tridion\website\DXA_Preview\web.config file 

		  <dependentAssembly>
			<assemblyIdentity name="Tridion.ConnectorFramework.Connector.SDK" publicKeyToken="ddfc895746e5ee6b" culture="neutral" />
			<bindingRedirect oldVersion="0.0.0.0-42.0.0.0" newVersion="42.2.1.0" />
		  </dependentAssembly>
		  
		  <dependentAssembly>
			<assemblyIdentity name="Tridion.Remoting.Contracts" publicKeyToken="ddfc895746e5ee6b" culture="neutral" />
			<bindingRedirect oldVersion="0.0.0.0-42.0.0.0" newVersion="42.2.1.0" />
		  </dependentAssembly>
		  
		   <dependentAssembly>
			<assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
			<bindingRedirect oldVersion="0.0.0.0-12.0.0.0" newVersion="13.0.0.0" />
		   </dependentAssembly>
		  		  
			<add key="form-field-namespaces" value="SFMC" />
			<add key="form-field-policies" value="JourneyEventTrigger=NameValueList, Contact=EntityFields" />
			<add key="form-objectkey-fieldnames" value="JourneyEventTrigger=eventDefinitionKey" />
		 
			<add key="session-adf-claims" value="Contact.Id, Contact.Journeys#ContactData[Email][0]>Email, Contact.Journeys#Name>Journeys, MarketoLead.Id, MarketoLead.ProgramMembership, MarketoLead.AdditionalFields[LeadScore]" />
			<add key="request-processor-marketo-namespace" value="marketo" />
			<add key="request-processor-marketo-enabled" value="false" />
			<add key="request-processor-sfmc-namespace" value="sfmc" />
			<add key="request-processor-sfmc-enabled" value="true" />
		  
	  
9. Update C:\tridion\Websites\DXA_Preview\bin\config\cd_ambient_conf.xml
			 
			  <Claim Uri="taf:claim:integration:contact:email"/>
			  <Claim Uri="taf:claim:integration:contact:name"/>
			  <Claim Uri="taf:claim:integration:contact:segment"/>
		
		
10. Update C:\tridion\Websites\DXA_Preview\bin\config\smarttarget_conf.xml in both the tags </GloballyAcceptedClaims> and  </ForwardedClaims>
				
				<taf_claim_integration_contact>contact</taf_claim_integration_contact>
				<taf_claim_integration_marketolead>marketolead</taf_claim_integration_marketolead>
        
		
11. Update the application properties in session 
				
				graphql.mutations.enabled=${mutationsenabled:false} to graphql.mutations.enabled=${mutationsenabled:true}



12. Add new xo triggers 

		PS C:\Users\rws> Get-XoSettings
		PS C:\Users\rws> Set-XoSettings -OpenSearchUrl "https://os.tridiondemo.com:9200" -Credential (Get-Credential)
		User name : ****
		Password : ****

		Run the below command :
		
		New-XoTriggerType -Id vis_segment -Name "Visitor segment" -BaseType Text -UrlParam "vis_segment" -MultiSelect -Values "Polycrystalline","Commercial","Residential"


13. Add the alternate text as "residential" to the component
	300-> Building Block > Content->demo> Banners > Light Solar CII Generation 2 - Product Details Page Banner


14. Add the alternate text as "homepage" to the component
	300-> Building Block > Content->demo> Banners > Homepage Banner

15. Create a new component lead generation form of type IntegrationForm schema that will be used when residential promition is triggered

16. Create a promition for residential 
		Where
			Publication : 500 publication
			Pages : XO Homepage
			PageRegions : Banner and Hero
		
		When to Trigger 
			Visitor segment is Residential
		
		What Content items 
			Residential Banner 
			component lead generation form
		 