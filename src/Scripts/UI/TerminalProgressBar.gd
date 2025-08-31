extends Node

## Reusable ASCII-style progress bar for terminal interfaces

# Progress bar characters
const FILLED_CHAR = "█"
const EMPTY_CHAR = "░"
const START_CHAR = "["
const END_CHAR = "]"

# Default settings
const DEFAULT_WIDTH = 20
const DEFAULT_DURATION = 2.0

# Maximum width to prevent wrapping on small screens
const MAX_WIDTH = 20

# Progress tracking
var _progress = 0.0
var _duration: float
var _width: int
var _is_running = false
var _elapsed_time = 0.0

# Callbacks
var on_progress_update: Callable
var on_complete: Callable

# Display components
var _terminal
var _label: String
var _message_type: int

func _init():
	process_mode = Node.PROCESS_MODE_ALWAYS

## Start a progress bar animation
func start_progress(terminal, label: String = "LOADING", duration: float = DEFAULT_DURATION, width: int = DEFAULT_WIDTH, message_type: int = 1):
	_terminal = terminal
	_label = label
	_duration = duration
	# Clamp width to prevent wrapping on small screens
	_width = min(width, MAX_WIDTH)
	_message_type = message_type
	_progress = 0.0
	_elapsed_time = 0.0
	_is_running = true
	
	# Draw initial state
	update_display()

## Stop the progress bar
func stop():
	_is_running = false
	_progress = 1.0
	update_display()

## Set progress manually (0.0 to 1.0)
func set_progress(progress: float):
	_progress = clamp(progress, 0.0, 1.0)
	update_display()

func _process(delta):
	if not _is_running or _terminal == null:
		return
	
	_elapsed_time += delta
	_progress = min(_elapsed_time / _duration, 1.0)
	
	update_display()
	if on_progress_update.is_valid():
		on_progress_update.call(_progress)
	
	if _progress >= 1.0:
		_is_running = false
		if on_complete.is_valid():
			on_complete.call()

func update_display():
	if _terminal == null:
		return
	
	# Calculate filled/empty blocks
	var filled_count = roundi(_progress * _width)
	var empty_count = _width - filled_count
	
	# Build progress bar string
	var progress_bar = START_CHAR
	for i in filled_count:
		progress_bar += FILLED_CHAR
	for i in empty_count:
		progress_bar += EMPTY_CHAR
	progress_bar += END_CHAR
	
	# Add percentage
	var percentage = roundi(_progress * 100)
	var display_text = "%s %s %d%%" % [_label, progress_bar, percentage]
	
	# Update terminal display
	_terminal.update_last_line(display_text, _message_type)

## Create a simple loading animation with callback
static func create_and_start(parent: Node, terminal, label: String = "LOADING", duration: float = DEFAULT_DURATION, on_complete_func: Callable = Callable()):
	var progress_bar = TerminalProgressBar.new()
	parent.add_child(progress_bar)
	
	progress_bar.on_complete = func():
		if on_complete_func.is_valid():
			on_complete_func.call()
		progress_bar.queue_free()
	
	progress_bar.start_progress(terminal, label, duration)
	return progress_bar
