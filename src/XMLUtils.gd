##
#Historically accurate educational video game based in 1830s Lausanne.
#Copyright (C) 2021  GameLab UNIL-EPFL
#
#This program is free software: you can redistribute it and/or modify
#it under the terms of the GNU General Public License as published by
#the Free Software Foundation, either version 3 of the License, or
#(at your option) any later version.
#
#This program is distributed in the hope that it will be useful,
#but WITHOUT ANY WARRANTY; without even the implied warranty of
#MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#GNU General Public License for more details.
#
#You should have received a copy of the GNU General Public License
#along with this program.  If not, see <http://www.gnu.org/licenses/>.
#
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
	
