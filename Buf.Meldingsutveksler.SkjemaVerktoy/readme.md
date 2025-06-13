# XML-/XSD-verkt�y bruk i Buf fagsystem (class library)
## Funksjonalitet
- Register med
	- XSD'er
	- Gyldigetstabell (fra-til-status) for XSDer (Lagres i blob containers)
- Validering
- XSD-kompilering
- Hente tekster
- Hente kodelistedata
- Generere gyldige meldinger fra XSD med testdata (men ikke n�dvendigvis med gode, illustrative data)

## Valideringsniv�:
1. "Plain C# library" - validering via innebygget metode i XmlDocument. Returnerer kun 1. feil, er dermed lite egnet for f.eks. en tilbakemelding til bruker som vil sende en kompleks melding.
2. Egenutviklet, komplett valdering av alle element (returnerer alle feil) + evaluering av egenutviklet <enable> - attributt som brukes for � spesifisere at et element er obligatorisk ved en gitt verdi i et annet element.
3. (Gj�res f�rst i fagsystem, ikke her) Validering av faglig innhold, som f.eks.: 
	- at barneverntjenester ikke kan ytes til en 30-�ring (med mindre det er en gravid mor)
	- at barnet det s�kes tiltak for allerede har et annet, ikke avsluttet tiltak og disse ikke kan kombineres
	- at det er noe feil med datoer (s�ke om tiltak med oppstartsdato i fjor, e.l.)

## Feilkoder
Feilkoder fra validering med tilh�rende beskrivelser "kompileres" til XSD (eller JSON) for publisering sammen med XSDer
- Kodes med valideringsniv� (se over)

## Brukes av
- Klient
- Testapplikasjoner
- Service (gr.leggende validering) (?)

## Konfigurasjonsbehov (i app som benytter library)
- Spec p� Blob container