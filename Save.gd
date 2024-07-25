extends Node

const SAVE_FILE_PATH = "user://GravityBladeSave.json" ###!!! change this to "user://save_data.json" when done testing (too much of a hastle for now). Manage save files through main menu (that's where you create a new empty save).


func export_to_dict(high_score : float):
	# print(InstrumentHandler.GetCurrentXPsAsGodotArray())
	return {
		"high_score": high_score
	}


func save_game(high_score : float):
	var save_game = File.new()
	save_game.open(SAVE_FILE_PATH, File.WRITE)
	var save_data = export_to_dict(high_score)
	save_game.store_line(to_json(save_data))
	save_game.close()


func load_game():
    var save_game = File.new()
    if !save_game.file_exists(SAVE_FILE_PATH):
        print("ERROR - save file does not exist (Save.gd)")
        return
    save_game.open(SAVE_FILE_PATH, File.READ)

	# var input = save_game.get_as_text()
    var save_data = parse_json(save_game.get_line())

    var world = get_node_or_null("/root/World")
    if (world != null and world is World):
        world.HighScore = save_data.high_score
	


func does_save_exist():
	var save_game = File.new()
	return save_game.file_exists(SAVE_FILE_PATH)

