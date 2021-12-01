import xml.etree.ElementTree as ET
import sys

##
# Represents the metadata of an xmltag
##
class XMLTagInfo:
    def __init__(self, tagname: str, attrnames: list) -> None:
        self.tag = tagname
        self.attrnames = attrnames

##
# Represents a filled in xmltag containing values for each attribute
##
class XMLTag(XMLTagInfo):
    def __init__(self, xmlinfo: XMLTagInfo, values: list) -> None:
        super().__init__(xmlinfo.tag, xmlinfo.attrnames)
        self.values = {}

        for i in range(len(self.attrnames)):
            if i < len(values):
                self.values[self.attrnames[i]] = values[i]

##
# Creates an XML file using the XMLTags created from the parsed tsv
# @param filename: str, the name of the input file
# @param values: list[list[XMLTag]], the list of xmlnodes retrieved from the tsv
# @param bodies: list[str], the actual text bodies retrieved from the tsv
##
def writeToXML(filename, values, bodies) -> None:

    # Create XML datatree
    rootTag: XMLTag = values[0][0]
    root = ET.Element(rootTag.tag, rootTag.values)
    bodyidx = 0

    # Iterate through each value line
    for xmlnodes in values:
        subelem: ET.Element = root

        # Iterate through each xmlnode in the line (excluding body)
        for j in range(1, len(xmlnodes) - 1):
            node = xmlnodes[j]
            subelem = ET.SubElement(subelem, node.tag, node.values)

        # Extract current body
        body: str = bodies[bodyidx]

        # Check for text follow-ups
        texts = body.split('&')
        textmeta = XMLTagInfo("text", [])
        for text in texts:
            # Add text tag
            textnode = XMLTag(textmeta, [])
            textsubelem = ET.SubElement(subelem, textnode.tag, textnode.values)

            # Check for options
            options = text.split('|')

            if len(options) > 1:
                optidx = 0
                optionmeta = XMLTagInfo("option", ["id"])
                # Loop through options and create a new node for each one
                for option in options:
                    xmlnode = XMLTag(optionmeta, [str(optidx)])
                    ET.SubElement(textsubelem, xmlnode.tag, xmlnode.values).text = option
                    optidx += 1
            else:
                textsubelem.text = text
            
        bodyidx += 1

    # Output file name is same as input but .xml
    outputname = "xml/" + filename.split('/')[1].split('.')[0] + ".xml"

    # Create tree
    tree = ET.ElementTree(root)
    tree.write(outputname)

##
# Parses the given .tsv file and converts it into an xml file
# @param filename: str, the name of the file to be parsed
##
def parseTSV(filename: str) -> None:
    tags: list[XMLTagInfo] = []
    values: list[list[XMLTag]] = []
    bodies: list[str] = []

    with open(filename) as file:
        lines = file.read().split('\n')
        idx = 0

        # Parse tsv
        for line in lines:
            # Separate by tabs
            tsv = line.split('\t')

            # Gather tag and attribute names
            if idx == 0:
                for elem in tsv:
                    # Separate tagname from attribute name
                    fields = elem.split(' ')

                    # Create xml metadata and push to list
                    xmlinfo = XMLTagInfo(fields[0], fields[1:])
                    tags.append(xmlinfo)
            # Gather values
            else:
                xmlnodes: list[XMLTag] = []
                tagidx = 0

                # Convert XMLInfo into XML tags
                for tag in tags:
                    attrvals = tsv[tagidx].split(' ')
                    xmlnode = XMLTag(tag, attrvals)
                    xmlnodes.append(xmlnode)
                    tagidx += 1
                
                # Add text to text array and attributes to attributes array
                bodies.append(tsv[len(tsv) - 1])
                values.append(xmlnodes)
            idx += 1
    
    # Write to XML file
    writeToXML(filename, values, bodies)

##
# Calls parseTSV, expects the filename to be given in the command line
##
if __name__ ==  "__main__":
    if len(sys.argv) < 2:
        raise Exception("No tsv found!")
    if sys.argv[1].split('.')[1] != "tsv": 
        raise Exception("Only .tsv format accepted!")

    parseTSV(sys.argv[1])
