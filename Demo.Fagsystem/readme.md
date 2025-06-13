# Fagsystem demo 
Utledet fra demoapp som brukes for utvikling av henvisningsskjema (https://demo-meldingsformater.azurewebsites.net/)
1 applikasjon som kan kjøre opp flere instanser som representerer ulike fagsystem: 
- internt Bufetat fagsystem (a la BiRK)
- eksternt fagsystem (kommune, privat leverandør)
## Fellesfunksjonalitet
- Bygger skjema UI basert på melding XSD
- Validering via SkjemaVerktoy lib.
- Kodelistetekster via SkjemaVerktoy lib
- Meldingslager i Blob container
	- Separat for hver "instans"
- Mockup folkeregister
## Buf klient:
- Bruker klienten (Buf.Meldingsutveksler.Klient) til å sende/motta meldinger
- Mockup saksregister (barn) basert på innkommende henvisninger
## Ekstern klient:
- Bruker Fiks I/O kliententil å sende/motta meldinger
- Mockup saksregister (barn) basert på testdata
 
