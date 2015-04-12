CKEDITOR.plugins.add( 'gapscreator', {
	// jscs:enable maximumLineLength
    icons: 'gapscreator', // %REMOVE_LINE_CORE%
    init: function( editor ) {
        editor.addCommand( 'gaps', new CKEDITOR.dialogCommand( 'gapsDialog' ) );
		editor.ui.addButton( 'insertGap', {
			label: 'Insert Gap',
			command: 'gaps',
			modes: { wysiwyg: 1 },
			icon: this.path + 'icons/gapscreator.png'
		});
		
		if ( editor.contextMenu ) {
			editor.addMenuGroup( 'gapsGroup' );
			editor.addMenuItem( 'gapsEditItem', {
				label: 'Edit Gap',
				icon: this.path + 'icons/gapscreator.png',
				command: 'gaps',
				group: 'gapsGroup'
			});
			
			editor.addMenuItem( 'gapsCreateItem', {
				label: 'Create Gap',
				icon: this.path + 'icons/gapscreator.png',
				command: 'gaps',
				group: 'gapsGroup'
			});
			
			editor.contextMenu.addListener( function( element ) {
				if ( element.getAscendant( 'abbr', true ) ) {
					return { gapsEditItem: CKEDITOR.TRISTATE_OFF };
				}
				else
				{
					var selection = editor.getSelection();
					return { gapsCreateItem: CKEDITOR.TRISTATE_OFF};
				}
			});
		}
		
		CKEDITOR.dialog.add( 'gapsDialog', this.path + 'dialogs/gapscreator.js' );
    }
});