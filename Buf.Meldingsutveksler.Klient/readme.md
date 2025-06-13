# Klient for bruk i Buf fagsystem (class library)
## Funksjonalitet
- Innkommende melding:
	- Lytter på (Azure) servicebus 
	- Laster ned melding fra (Azure) Blob container
	- Laster ned evt. vedlegg fra Blob container
	- Kvitterer ved å:
		- slette melding fra servicvebus
		- slette meldingsinnhold og vedlegg fra Blob storage
  - Sender applikasjonskvittering 

- Utgående melding:
	- Legger vedlegg og melding i blob
	- Legger melding i servicebus
	- Lytter på event "melding sendt?"
	- registrerer mottatt applikasjonskvittering

- Xml/Xsd - verktøy:
	- Kompilere XSD
	- Hente XSD og tekster (Blob container)
	- Validere XML
		- "Plain" C#-library validering
		- Utvidet validering

## Konfigurasjon
- Spec på Blob container for XSD-lager (brukes av SkjemaVerktøy-library)
- Spec på Servicebus
- Spec på Blob container for meldinger/vedlegg)
- Sertifikat for kryptering / dekryptering