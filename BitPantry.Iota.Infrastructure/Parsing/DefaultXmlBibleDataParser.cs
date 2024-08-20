using Azure.Core;
using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

/***

Parses data that matches the following XSD schema ---------------------------------------------------------------------
 
<?xml version="1.0" encoding="UTF-8"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
           elementFormDefault="qualified"
           attributeFormDefault="unqualified">

  <!-- Root element: Bible -->
  <xs:element name="bible">
    <xs:complexType>
      <xs:sequence>
        <xs:element name="testament" maxOccurs="2">
          <xs:complexType>
            <xs:sequence>
              <xs:element name="book" maxOccurs="unbounded">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="chapter" maxOccurs="unbounded">
                      <xs:complexType>
                        <xs:sequence>
                          <xs:element name="verse" maxOccurs="unbounded">
                            <xs:complexType>
                              <xs:simpleContent>
                                <xs:extension base="xs:string">
                                  <xs:attribute name="number" type="xs:int" use="required"/>
                                </xs:extension>
                              </xs:simpleContent>
                            </xs:complexType>
                          </xs:element>
                        </xs:sequence>
                        <xs:attribute name="number" type="xs:int" use="required"/>
                      </xs:complexType>
                    </xs:element>
                  </xs:sequence>
                  <xs:attribute name="number" type="xs:int" use="required"/>
                </xs:complexType>
              </xs:element>
            </xs:sequence>
            <xs:attribute name="name" use="required">
              <xs:simpleType>
                <xs:restriction base="xs:string">
                  <xs:enumeration value="Old"/>
                  <xs:enumeration value="New"/>
                </xs:restriction>
              </xs:simpleType>
            </xs:attribute>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
      <xs:attribute name="translationShortName" type="xs:string" use="required"/>
      <xs:attribute name="translationLongName" type="xs:string" use="required"/>
      <xs:attribute name="classification" use="required">
        <xs:simpleType>
          <xs:restriction base="xs:string">
            <xs:enumeration value="Protestant"/>
            <xs:enumeration value="Catholic"/>
          </xs:restriction>
        </xs:simpleType>
      </xs:attribute>
      <xs:attribute name="description" type="xs:string" use="optional"/>
    </xs:complexType>
  </xs:element>

</xs:schema>



Here's an example XML document that exemplifies this schema ------------------------------------------------------------------------

<?xml version="1.0" encoding="UTF-8"?>
<bible translationShortName="ESV" translationLongName="English Standard Version" classification="Protestant" description="This is a sample description of the Bible.">
  <testament name="Old">
    <book number="1">
      <chapter number="1">
        <verse number="1">In the beginning, God created the heavens and the earth.</verse>
      </chapter>
    </book>
  </testament>
  <testament name="New">
    <book number="40">
      <chapter number="1">
        <verse number="1">The book of the genealogy of Jesus Christ, the son of David, the son of Abraham.</verse>
      </chapter>
    </book>
  </testament>
</bible>

*/



namespace BitPantry.Iota.Infrastructure.Parsing
{
    public class DefaultXmlBibleDataParser : IBibleParser
    {
        public Bible Parse(string dataFilePath)
        {
            var xmlDoc = XDocument.Load(dataFilePath);

            // Parse and validate the classification attribute
            var classificationValue = xmlDoc.Root.Attribute("classification")?.Value;
            if (!Enum.TryParse<BibleClassification>(classificationValue, out var classification) || !Enum.IsDefined(typeof(BibleClassification), classification))
            {
                throw new BibleDataParsingException($"Invalid classification value: '{classificationValue}'. Must be one of {string.Join(", ", Enum.GetNames(typeof(BibleClassification)))}.");
            }

            // Parse and validate the translation attributes
            var translationShortName = xmlDoc.Root.Attribute("translationShortName")?.Value;
            var translationLongName = xmlDoc.Root.Attribute("translationLongName")?.Value;

            // Parse the description element (optional)
            var description = xmlDoc.Root.Attribute("description")?.Value;

            // Parse and validate the testaments
            var testaments = xmlDoc.Root.Elements("testament").Select(t =>
            {
                var testamentNameValue = t.Attribute("name")?.Value;
                if (!Enum.TryParse<TestamentName>(testamentNameValue, out var testamentName) || !Enum.IsDefined(typeof(TestamentName), testamentName))
                {
                    throw new BibleDataParsingException($"Invalid testament name value: '{testamentNameValue}'. Must be one of {string.Join(", ", Enum.GetNames(typeof(TestamentName)))}.");
                }

                return new Testament
                {
                    Name = testamentName,
                    Books = t.Elements("book").Select(b => new Book
                    {
                        Number = int.Parse(b.Attribute("number")?.Value),
                        Chapters = b.Elements("chapter").Select(c => new Chapter
                        {
                            Number = int.Parse(c.Attribute("number")?.Value),
                            Verses = c.Elements("verse").Select(v => new Verse
                            {
                                Number = int.Parse(v.Attribute("number")?.Value),
                                Text = v.Value
                            }).ToList()
                        }).ToList()
                    }).ToList()
                };
            }).ToList();

            return new Bible
            {
                TranslationShortName = translationShortName,
                TranslationLongName = translationLongName,
                Classification = classification,
                Description = description, 
                Testaments = testaments
            };
        }
    }
}
