## Salesforce Marketing Cloud Journey 

- Login to https://mc.login.exacttarget.com/

- Create a Dataextension in Contact Builder
		Attributes like 
			FirstName
			LastName
			Email
			Appointment
			
- Create a email template to send email.
			In the email body set the below url and query string
			https://sfmc-preview.tridiondemo.com/xohome.html?ContactKey=%%_subscriberkey%%&utm_campaign=Residential
			
- Create a journey builder
		with few steps like send email attach the data extension to save the data.
		

		
