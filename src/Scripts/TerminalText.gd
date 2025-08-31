extends CanvasLayer

const CHARACTER_READ_RATE = 0.025

signal terminal_text_finished

enum TerminalState {
	READY,
	READING,
	FINISHED
}

var _textbox_container: MarginContainer
var _label: Label
var _tween: Tween
var _current_state = TerminalState.READY
var _queued_texts: Array[String] = []

func _ready():
	print("Starting state: TerminalState.%s" % TerminalState.keys()[_current_state])
	initialize_tween()
	_textbox_container = get_node("TextboxContainer")
	_label = get_node("TextboxContainer/MarginContainer/HBoxContainer/Label")
	hide_text_box()
	queue_text("       **** GHOSTD V1 ****\nBOOTING...\nINITIALIZING 128G RAM...\n    - 124G FREE\nLOADING TERMINAL...\n\nREADY.")

func _process(_delta):
	match _current_state:
		TerminalState.READY:
			if _queued_texts.size() > 0:
				display_text()
		TerminalState.READING:
			# Waiting for user input to finish reading
			pass
		TerminalState.FINISHED:
			pass
		_:
			push_error("TestBoot: Unknown state %s" % _current_state)

func _input(event):
	if (event is InputEventScreenTouch and event.is_pressed()) or \
	   (event is InputEventMouseButton and event.is_pressed()):
		if _current_state == TerminalState.READING:
			print("TestBoot: User pressed a screen, finishing text reading")
			_label.visible_ratio = 1 # Show all text immediately
			_tween.stop()
			_on_tween_finished()
		elif _current_state == TerminalState.FINISHED:
			if _queued_texts.size() > 0:
				print("TestBoot: Tween finished, displaying next text")
				initialize_tween()
				display_text()

func queue_text(next_text: String):
	_queued_texts.append(next_text)

func hide_text_box():
	_label.text = ""
	_textbox_container.hide()

func show_text_box():
	_textbox_container.show()

func display_text():
	var next_text = _queued_texts.pop_front()
	_label.text = next_text
	change_state(TerminalState.READING)
	show_text_box()
	_label.visible_ratio = 0
	_tween.tween_property(
		_label,
		"visible_ratio",
		1,
		next_text.length() * CHARACTER_READ_RATE)

func _on_tween_finished():
	change_state(TerminalState.FINISHED)
	print("Ending state: TerminalState.%s" % TerminalState.keys()[_current_state])
	
	if _queued_texts.size() == 0:
		# Signal that the terminal session is complete
		terminal_text_finished.emit()

func change_state(next_state: TerminalState):
	_current_state = next_state
	print("Changing state to TerminalState.%s" % TerminalState.keys()[next_state])

func initialize_tween():
	_tween = get_tree().create_tween()
	_tween.finished.connect(_on_tween_finished)
