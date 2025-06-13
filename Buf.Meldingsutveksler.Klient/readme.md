# Klient for bruk i Buf fagsystem (class library)
## Funksjonalitet
- Innkommende melding:
	- Lytter p� (Azure) servicebus 
	- Laster ned melding fra (Azure) Blob container
	- Laster ned evt. vedlegg fra Blob container
	- Kvitterer ved �:
		- slette melding fra servicvebus
		- slette meldingsinnhold og vedlegg fra Blob storage
  - Sender applikasjonskvittering 

- Utg�ende melding:
	- Legger vedlegg og melding i blob
	- Legger melding i servicebus
	- Lytter p� event "melding sendt?"
	- registrerer mottatt applikasjonskvittering

- Xml/Xsd - verkt�y:
	- Kompilere XSD
	- Hente XSD og tekster (Blob container)
	- Validere XML
		- "Plain" C#-library validering
		- Utvidet validering

## Konfigurasjon
- Spec p� Blob container for XSD-lager (brukes av SkjemaVerkt�y-library)
- Spec p� Servicebus
- Spec p� Blob container for meldinger/vedlegg)
- Sertifikat for kryptering / dekryptering