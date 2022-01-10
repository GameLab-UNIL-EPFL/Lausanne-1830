extends Node

onready var xmlp := XMLParser.new()
onready var xmlPBA = PoolByteArray()
	
func parseXML(filepath: String) -> XMLParser:
	
	var err = xmlp.open(filepath)
	
	# Sanity check
	if err != OK:
		print(err + ": Error opening the xml file!")
		return null
		
	err = xmlp.open_buffer(xmlPBA)
	if err != OK:
		print(err + ": Error opening the xml buffer!")
		return null
		
	return xmlp
	
# Query the parsed XML file and return a dictionary assotiating fields to values
func queryXML():
	var res = {}
		
	while xmlp.read() != ERR_FILE_EOF:
		print(xmlp.get_node_name(), ": ", xmlp.get_node_data())
		if xmlp.get_attribute_count() > 0:
			for i in range(xmlp.get_attribute_count()):
				print(xmlp.get_attribute_name(i), ": ", xmlp.get_attribute_value(i))
				res[xmlp.get_attribute_name(i)] = xmlp.get_attribute_value(i)

	print(res)
	return res

func exit(error) -> void:
	get_tree().quit()
	
