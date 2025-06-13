using Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste;
using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll;
using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll.Models;
using Buf.Meldingsutveksler.SkjemaVerktoy.Tekster;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using Demo.Fagsystem.Models.Business;
using Demo.Fagsystem.Models.FagsystemSimulator.Config;
using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using Demo.Fagsystem.Models.Utils;
using Demo.Fagsystem.Pages.Partial;
using System.Xml.Schema;

namespace Demo.Fagsystem.Models.Demodata
{
    public static class DemodataGenerator
    {
        public static void InitFagsystemer(IConfiguration config)
        {
            // Add Asker komm. bvtjeneste
            var configs = config.GetSection("Fagsystem").Get<FagsystemConfigBase[]>();
            var kommuneConfig = configs?.FirstOrDefault(c => c.Id == "KOMMUNE");
            FagsystemBase fsKommune = new(GetFagsystemInfoKommune(), GetBarneverntjeneste(), GetKommuneBruker(), kommuneConfig)
            {
                Klienter = GenerateTestBarn(),
                PageFolder = "Kommune"
            };
            FagsystemAccessor.FagsystemInstanser.Add(fsKommune);

            var bufConfig = configs?.FirstOrDefault(c => c.Id == "BUFETAT");
            FagsystemBase fsBuf = new(GetFagsystemInfoBuf(), GetBufetat(), GetBufetatBruker(), bufConfig)
            {
                PageFolder = "Bufetat"
            };
            FagsystemAccessor.FagsystemInstanser.Add(fsBuf);

        }

        private static FagsystemBruker GetKommuneBruker()
        {
            FagsystemBruker result = new()
            {
                KontaktInfo = new()
                {
                    Kontaktperson = new()
                    {
                        Navn = "Ann Berver",
                        epost = "ann.berver@asker.kommune.no",
                        Telefon = "90099009"
                    },
                    KontaktpersonLeder = new()
                    {
                        Navn = "Lena Brevandrer",
                        epost = "lena.brevandrer@asker.kommune.no",
                        Telefon = "90099010"
                    },
                }
            };
            return result;
        }

        public static List<Organisasjon> GetTestorganisasjoner()
        {
            List<Organisasjon> result = [GetBarneverntjeneste(), GetBufetat(), GetBufDir()];
            return result;
        }

        private static List<FREG_Person> addFregData()
        {
            List<FREG_Person> personer =
            [
                new()
                {
                    Id = 10000001,
                    Fornavn = "Bjarne",
                    Etternavn = "Wern",
                    Fodselsdato = new DateTime(2012, 12, 12),
                    Fodselsnummer = "12121241555",
                    Graderingsnivaa = AdresseGradering.ugradert,
                    Kjonn = "mann"
                },
                new()
                {
                    Id = 10000002,
                    Fornavn = "Birgitte",
                    Etternavn = "Wern",
                    Fodselsdato = new DateTime(2010, 10, 10),
                    Fodselsnummer = "10101041444",
                    Graderingsnivaa = AdresseGradering.ugradert,
                    Kjonn = "kvinne"
                },
                new()
                {
                    Id = 10000003,
                    Fornavn = "Rigmor",
                    Etternavn = "Wern",
                    Fodselsdato = new DateTime(1990, 10, 10),
                    Fodselsnummer = "10109051455",
                    Graderingsnivaa = AdresseGradering.ugradert,
                    Kjonn = "kvinne"
                },
                new()
                {
                    Id = 10000004,
                    Fornavn = "Rigfar",
                    Etternavn = "Hermann",
                    Fodselsdato = new DateTime(1990, 12, 12),
                    Fodselsnummer = "12129051555",
                    Graderingsnivaa = AdresseGradering.ugradert,
                    Kjonn = "mann"
                },
                new()
                {
                    Id = 10000005,
                    Fornavn = "Alexander",
                    Etternavn = "Hermann",
                    Fodselsdato = new DateTime(1949, 02, 02),
                    Fodselsnummer = "02024953555",
                    Graderingsnivaa = AdresseGradering.ugradert,
                    Kjonn = "mann"
                }
                ];
            return personer;
        }

        private static List<Klient> addBarn()
        {
            List<Klient> barn =
            [
                new Barn()
                {
                    Id = 100010001,
                    _FREG_Person_Id = 10000001,
                    Type = KlientType.BarnevernStandard,
                    Nettverk = [
                        new NettverkPerson()
                        {
                            Id = 12000001,
                            Sak_Id = 100010001,
                            NettverkRelasjon = NettverkRelasjonType.Mor,
                            FREG_Person_id = 10000003
                        },
                        new NettverkPerson()
                        {
                            Id = 12000002,
                            Sak_Id = 100010001,
                            NettverkRelasjon = NettverkRelasjonType.Far,
                            FREG_Person_id = 10000004
                        },
                        new NettverkPerson()
                        {
                            Id = 12000003,
                            Sak_Id = 100010001,
                            NettverkRelasjon = NettverkRelasjonType.Annet,
                            FREG_Person_id = 10000002
                        },
                        new NettverkPerson()
                        {
                            Id = 12000004,
                            Sak_Id = 100010001,
                            NettverkRelasjon = NettverkRelasjonType.Annet,
                            FREG_Person_id = 10000005
                        }
                    ]
                },
                new Barn()
                {
                    Id = 100010002,
                    _FREG_Person_Id = 10000003,
                    Type = KlientType.BarnevernUfodt,
                    Nettverk = [
                        new NettverkPerson()
                        {
                            Id = 12000005,
                            Sak_Id = 100010002,
                            NettverkRelasjon = NettverkRelasjonType.Mor,
                            FREG_Person_id = 10000003
                        },
                        new NettverkPerson()
                        {
                            Id = 12000002,
                            Sak_Id = 100010002,
                            NettverkRelasjon = NettverkRelasjonType.Far,
                            FREG_Person_id = 10000004
                        }
                    ]
                }
            ];
            return barn;
        }

        private static List<Sak> addSaksdata()
        {
            List<Sak> saker =
                [
                    new Sak() // Bjarne Wern
                    {
                        Id = 10000001,
                        KlientId = 10000001,
                        Saksdata = new Dictionary<string, string>()
                        {
                            { "FellesInfo.BarnMedNettverk.BarnetsSituasjon.Omrade", "Omrade=1|Beskrivelse=" },
                        }
                    },

                    new Sak() // Birgitte Wern
                    {
                        Id = 10000002,
                        KlientId = 10000002,
                    }
                    ];
            return saker;
        }

        private static void initBarn(List<Klient> barn, List<FREG_Person> freg)
        {
            foreach (Barn b in barn.Cast<Barn>())
            {
                b.FREG_Person = freg.FirstOrDefault(f => f.Id == b._FREG_Person_Id);
                foreach (NettverkPerson np in b.Nettverk ?? [])
                {
                    np.fREG_Person = freg.FirstOrDefault(f => f.Id == np.FREG_Person_id);
                }
            }
        }

        public static List<Klient> GenerateTestBarn()
        {
            var freg = addFregData();
            var barn = addBarn();
            initBarn(barn, freg);
            return barn;
        }

        public static Organisasjon GetBarneverntjeneste()
        {
            return new Organisasjon()
            {
                Navn = "Barneverntjenesten i Asker",
                Kortnavn = "Asker BV",
                Organisasjonsnummer = "974635453",
                Kommuneinfo = new Kommuneinfo() { Kommunenavn = "Asker", Kommunenummer = "3203" },
                Aktortype = AktorType.Barnevern1Linje
            };
        }

        public static Organisasjon GetBufetat()
        {
            return new Organisasjon()
            {
                Navn = "Barne- ungdoms- og familieetaten",
                Kortnavn = "Bufetat",
                Organisasjonsnummer = "986128433",
                Aktortype = AktorType.Barnevern2Linje
            };
        }

        public static FagsystemBruker GetBufetatBruker()
        {
            FagsystemBruker result = new()
            {
                //Rolle = FagsystemBrukerRolle.ITadmin,
                KontaktInfo = new()
                {
                    Kontaktperson = new()
                    {
                        Navn = "Bjørge Sæther",
                        epost = "bjorge.saether@bufdir.no",
                        Telefon = "90822239"
                    },
                    KontaktpersonLeder = new()
                    {
                        Navn = "Kenneth Normann Hansen",
                        epost = "kenneth.hansen@bufdir.no",
                        Telefon = "90000000"
                    }
                }
            };
            return result;
        }

        public static Organisasjon GetBufDir()
        {
            return new Organisasjon()
            {
                Navn = "Barne- ungdoms- og familiedirektoratet",
                Kortnavn = "BufDir",
                Organisasjonsnummer = "986128433",
                Aktortype = AktorType.System
            };
        }

        public static FagsystemInfo GetFagsystemInfoBuf()
        {
            return new()
            {
                Leverandor = "Bufdir/LU",
                Navn = "KiRB",
                Versjon = "0.9.99"
            };
        }

        public static FagsystemInfo GetFagsystemInfoKommune()
        {
            return new()
            {
                Leverandor = "VisComp",
                Navn = "Flytulus",
                Versjon = "0.9.9"
            };
        }

        private static void UpdateValue(List<PrefilledValue> values, string element, string value, bool addIfNonExisting)
        {
            var item = values.FirstOrDefault(pfv => pfv.Xpath == element);
            if (item == null)
            {
                if (addIfNonExisting)
                {
                    item = new(element, value, true, false);
                    values.Add(item);
                }
            }
            else
                item.Value = value;
        }

        public static DateTime Update_SendtTidspunkt(Dictionary<string, string> values, string rootElementName)
        {
            DateTime value = DateTime.Now;
            values[$"{rootElementName}.Meldingshode.SendtTidspunkt"] = value.ToString(XmlUtils.DateTimeFormatXML);
            return value;
        }

        public static void AddOrUpdate(this List<PrefilledValue> list, PrefilledValue rec)
        {
            var existing = list.FirstOrDefault(pfl => pfl.Xpath == rec.Xpath);
            if (existing != null)
            {
                existing.Value = rec.Value;
                existing.InitiallyClosed = rec.InitiallyClosed;
                existing.OpenToEdit = rec.OpenToEdit;
            }
            else
                list.Add(rec);
        }

        public static void Update_MeldingsInfo(List<PrefilledValue> values, string aksjon, XmlSchema schema, Meldingshode? existing)
        {
            XmlSchemaElement rootElement = XsdUtils.GetRootElement(schema)
                ?? throw new Exception($"Ingen rotelement i schema '{schema.TargetNamespace}'");
            string caption = TeksterUtils.GetCaption(schema, rootElement, true);
            string rootElementName = rootElement.Name!;
            if (aksjon != "")
            {
                if (aksjon != "Ny")
                {
                    var aksjonEnum = XsdUtils.GetTypeDefinition(schema, Konstanter.MeldingsForbindelseType) ??
                        throw new Exception($"Finner ikke aksjonsverdier for type {Konstanter.MeldingsForbindelseType}");
                    var kodeliste = KodelisteUtils.GetKodeliste(aksjonEnum) ??
                        throw new Exception($"Finner ikke kodeliste for type {XsdUtils.GetName(aksjonEnum)}");
                    if (kodeliste.koder == null)
                        throw new Exception($"Finner ikke koder i kodeliste {XsdUtils.GetName(aksjonEnum)}");
                    var kode = kodeliste.koder.FirstOrDefault(kvp => kvp.verdi == aksjon)
                        ?? throw new Exception($"Meldingsforbindelse '{aksjon}' finnes ikke i kodeliste");
                    var varRetning = kode.variabler.First(v => v.navn == Konstanter.MeldingsForbindelseType_Variabel_Retning)
                        ?? throw new Exception($"Variabel '{Konstanter.MeldingsForbindelseType_Variabel_Retning}' ikke funnet i kodeliste '{Konstanter.MeldingsForbindelseType}'");
                    values.AddOrUpdate(new($"{rootElementName}.Meldingshode.OppfolgingAvMelding.MeldingsForbindelse", aksjon, false));
                    if (existing != null)
                    {
                        if (existing.Id == "")
                            throw new Exception("Finner ikke 'Id' i meldingshode");
                        string startId = existing.Id ?? "";
                        if ((existing.OppfolgingAvMelding?.StartMeldingId ?? "") != "")
                        {
                            startId = existing.OppfolgingAvMelding?.StartMeldingId ?? "";
                            string gjelderId = existing.OppfolgingAvMelding?.MeldingId ?? "";
                            values.AddOrUpdate(new($"{rootElementName}.Meldingshode.OppfolgingAvMelding.StartMeldingId", startId, false));
                            values.AddOrUpdate(new($"{rootElementName}.Meldingshode.OppfolgingAvMelding.MeldingId", gjelderId, false));
                        }
                        if (varRetning.verdi == "mottatt")
                        {
                            if (existing.AvsendersRef != "")
                                // melding er oppfølging av tidligere melding sendt av OSS
                                values.AddOrUpdate(new($"{rootElementName}.Meldingshode.OppfolgingAvMelding.MottakersRef", existing.AvsendersRef, false));
                        }
                    }
                }

            }
            values.AddOrUpdate(new($"{rootElementName}.Meldingshode.MeldingstypeNmsp", schema.TargetNamespace!, false, false));
            values.AddOrUpdate(new($"{rootElementName}.Meldingshode.Meldingstype", caption, false, false));
            values.AddOrUpdate(new($"{rootElementName}.Meldingshode.Id", Guid.NewGuid().ToString(), false));
        }

        private static void SetDisabledAndClosedElements(List<PrefilledValue> values, string path, bool openToEdit)
        {
            var affectedItems = values.Where(pfv => pfv.Xpath.StartsWith(path)).ToList();
            foreach (var item in affectedItems)
            {
                item.OpenToEdit = openToEdit;
            }
        }

        public static void UpdateEditableAndClosed(List<PrefilledValue> values, string rootElement)
        {
            SetDisabledAndClosedElements(values, $"{rootElement}.Meldingshode", false);
            var Meldingshode = values.FirstOrDefault(pfv => pfv.Xpath == $"{rootElement}.Meldingshode");
            if (Meldingshode == null)
                values.Add(new($"{rootElement}.Meldingshode", "", false, true));
            else
                Meldingshode.InitiallyClosed = true;
            SetDisabledAndClosedElements(values, $"{rootElement}.Meldingshode.KontaktInfoAvsender", true);
            SetDisabledAndClosedElements(values, $"{rootElement}.Klient.Identifikator", false);
            SetDisabledAndClosedElements(values, $"{rootElement}.Klient.KommunalSaksId", false);
        }

        public static List<PrefilledValue> GetPrefillValues(HttpRequest request, Meldingshode? existingMeldingshode, FagsystemInfo fagsystem, Organisasjon avsender, Organisasjon mottaker, KontaktInfo? kontaktinfo)
        {
            List<PrefilledValue> result = [];

            var selectedSchema = WebUtils.GetRequestValue(request, Konstanter.SelectedSkjema) ?? "";
            var schemaRec = XmlSchemaRegister.Schemas.xsds.FirstOrDefault(s => s.nmsp == selectedSchema);
            var schema = XsdUtils.GetSchema(selectedSchema);
            var rootElement = XsdUtils.GetRootElement(schema)
                ?? throw new Exception($"Schema {schema.TargetNamespace} inneholder ikke noe rotelement");
            var rootElementName = rootElement.Name;
            var caption = TeksterUtils.GetCaption(schema, rootElement, true);

            var selectedAksjon = WebUtils.GetRequestValue(request, Konstanter.SelectedAksjon) ?? "Ny";

            Update_MeldingsInfo(result, selectedAksjon, schema, existingMeldingshode);

            SakSelectorModel model = new(request);
            var barn = model.selectedKlient;

            result.Add(new($"{rootElementName}.Meldingshode", "", false, true));
            result.Add(new($"{rootElementName}.Meldingshode.MeldingstypeNmsp", schema.TargetNamespace!, false));
            result.Add(new($"{rootElementName}.Meldingshode.SendtTidspunkt", "", false)); // for at elementet skal være der
            result.Add(new($"{rootElementName}.Meldingshode.Meldingstype", caption, false));
            result.Add(new($"{rootElementName}.Meldingshode.AvsendersRef", barn?.Id.ToString() ?? "", false));
            result.Add(new($"{rootElementName}.Meldingshode.OppfolgingAvMelding", "", false));

            result.Add(new($"{rootElementName}.Meldingshode.FagsystemAvsender.Leverandor", fagsystem.Leverandor, false));
            result.Add(new($"{rootElementName}.Meldingshode.FagsystemAvsender.Navn", fagsystem.Navn, false));
            result.Add(new($"{rootElementName}.Meldingshode.FagsystemAvsender.Versjon", fagsystem.Versjon, false));

            AddOrgInfo(result, $"{rootElementName}.Meldingshode.Avsender", avsender);
            AddOrgInfo(result, $"{rootElementName}.Meldingshode.Mottaker", mottaker);

            if (kontaktinfo != null)
            {
                AddKontaktperson(result, $"{rootElementName}.Meldingshode.KontaktInfoAvsender.Kontaktperson", kontaktinfo.Kontaktperson);
                AddKontaktperson(result, $"{rootElementName}.Meldingshode.KontaktInfoAvsender.KontaktpersonLeder", kontaktinfo.KontaktpersonLeder);
            }

            result.Add(new($"{rootElementName}.Klient.Identifikator.Fodselsnummer", barn?.FREG_Person?.Fodselsnummer ?? "", false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.Fodseldato", barn?.FREG_Person?.Fodselsdato.ToString("yyyy-MM-dd") ?? "", false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.Kjonn", barn?.FREG_Person?.Kjonn ?? "", false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.EMA_FALSE", false.ToString(), false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.TerminDato", "", false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.Ufodt_FALSE", false.ToString(), false));
            result.Add(new($"{rootElementName}.Klient.Identifikator.DUFnummer", "", false));
            result.Add(new($"{rootElementName}.Klient.KommunalSaksId", DateTime.Now.Year.ToString() + "-" + barn?.FREG_Person?.Fodselsnummer, false));
            result.Add(new($"{rootElementName}.TiltakHistorikk", @"01.02.2022 - 12.09.2022 Fosterhjem
12.09.2022 - 20.10.2023 Omsorgsinstitusjon", true));
            return result;

        }

        private static void AddOrgInfo(List<PrefilledValue> result, string path, Organisasjon org)
        {
            result.Add(new($"{path}.Organisasjonsnummer", org.Organisasjonsnummer ?? "", false));
            result.Add(new($"{path}.Navn", org.Navn ?? "", false));
            result.Add(new($"{path}.Kommuneinfo", (org.Kommuneinfo != null).ToString(), false));
            result.Add(new($"{path}.Kommuneinfo.Kommunenummer", org.Kommuneinfo?.Kommunenummer ?? "", false));
            result.Add(new($"{path}.Kommuneinfo.Kommunenavn", org.Kommuneinfo?.Kommunenavn ?? "", false));
            result.Add(new($"{path}.Kommuneinfo.Bydelsinfo", (org.Kommuneinfo?.Bydelsinfo != null).ToString(), false));
            result.Add(new($"{path}.Kommuneinfo.Bydelsinfo.Bydelsnummer", org.Kommuneinfo?.Bydelsinfo?.Bydelsnavn ?? "", false));
            result.Add(new($"{path}.Kommuneinfo.Bydelsinfo.Bydelsnavn", org.Kommuneinfo?.Bydelsinfo?.Bydelsnavn ?? "", false));
        }

        private static void AddKontaktperson(List<PrefilledValue> result, string path, Kontaktperson? person)
        {
            result.Add(new($"{path}.Navn", person?.Navn ?? "-navn-", true));
            result.Add(new($"{path}.Telefon", person?.Telefon ?? "-tlf-", true));
            result.Add(new($"{path}.epost", person?.epost ?? "x@y.z", true));
        }
        internal static void GetAvsenderMottaker(FagsystemBase instans, XmlSchema schema, out Organisasjon? avsender, out Organisasjon? mottaker)
        {
            List<Organisasjon> aktorer = GetTestorganisasjoner();
            avsender = aktorer.FirstOrDefault(o => o.Organisasjonsnummer == instans.Organisasjon.Organisasjonsnummer);
            var rootElement = XsdUtils.GetRootElement(schema);
            string aktorTypeMottaker = XsdUtils.GetAppInfoValue(rootElement, Konstanter.AktorMottakerType, false, false);
            string[] aktorTyper = aktorTypeMottaker.Split('|');
            List<AktorType> aktTyper = MeldingsprotokollUtils.ArrayToAktorTyper(aktorTyper);
            var avsenderOrgNr = avsender?.Organisasjonsnummer;
            mottaker = aktorer.FirstOrDefault(o => o.Organisasjonsnummer != avsenderOrgNr && aktTyper.Contains(AktorType.All) || aktTyper.Contains(o.Aktortype));
        }
    }
}