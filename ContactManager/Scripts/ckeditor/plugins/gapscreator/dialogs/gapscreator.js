CKEDITOR.dialog.add( 'gapsDialog', function( editor ) {
    return {
        title: 'Gap',
        minWidth: 400,
        minHeight: 150,
        contents: [
            {
                id: 'tab-basic',
                label: 'Basic Settings',
                elements: [
                    {
                        type: 'text',
                        id: 'gap',
                        label: 'Text in gap',
                        validate: CKEDITOR.dialog.validate.notEmpty( "Text field cannot be empty." ),
						
						setup: function( element ) {
                            this.setValue( element.getText() );
                        },

                        commit: function( element ) {
                            element.setText( this.getValue() );
                        }
                    },
                    {
                        type: 'text',
                        id: 'title',
                        label: 'Explanation',
                        //validate: CKEDITOR.dialog.validate.notEmpty( "Explanation field cannot be empty." ),
						
						setup: function( element ) {
                            this.setValue( element.getAttribute( "title" ) );
                        },

                        commit: function( element ) {
                            element.setAttribute( "title", this.getValue() );
                        }
                    }
                ]
            }
        ],
		onShow: function() {
			var selection = editor.getSelection();
			var element = selection.getStartElement();

            if ( element )
                element = element.getAscendant( 'abbr', true );

            if ( !element || element.getName() != 'abbr' ) {
                element = editor.document.createElement( 'abbr' );
				element.setText(selection.getSelectedText());
				element.setAttribute('class', 'gaps');
				element.setAttribute('style', 'border-bottom: 1px dotted; text-decoration: none');
                this.insertMode = true;
            }
            else
                this.insertMode = false;

            this.element = element;
            
            this.setupContent( this.element );
		},
        onOk: function() {
            var dialog = this;

            var gap = this.element;
			this.commitContent(gap);
            
			if (this.insertMode)
				editor.insertElement( gap );
        }
    };
});