# Fagsystem demo 
Utledet fra demoapp som brukes for utvikling av henvisningsskjema (https://demo-meldingsformater.azurewebsites.net/)
1 applikasjon som kan kj�re opp flere instanser som representerer ulike fagsystem: 
- internt Bufetat fagsystem (a la BiRK)
- eksternt fagsystem (kommune, privat leverand�r)
## Fellesfunksjonalitet
- Bygger skjema UI basert p� melding XSD
- Validering via SkjemaVerktoy lib.
- Kodelistetekster via SkjemaVerktoy lib
- Meldingslager i Blob container
	- Separat for hver "instans"
- Mockup folkeregister
## Buf klient:
- Bruker klienten (Buf.Meldingsutveksler.Klient) til � sende/motta meldinger
- Mockup saksregister (barn) basert p� innkommende henvisninger
## Ekstern klient:
- Bruker Fiks I/O kliententil � sende/motta meldinger
- Mockup saksregister (barn) basert p� testdata
 
