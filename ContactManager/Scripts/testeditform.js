var currentType;
var currentQuestion;
var questions = [];
$(document).ready(function () {
    if (q) {
        q = escapeSpecialChars(q);
        questions = JSON.parse(q);
    }
    $(".chosen").chosen({ allow_single_deselect: true, width: "500px" });
    $("#additem").change(function () {
        selectedQuestion = 0;
        var type = $(this).val();
        switch (type) {
            case "text":
                $("#modalTextQuestion").modal("show");
                break;
            case "radio":
                $("#modalMultipleChoiceQuestion").modal("show");
                break;
            case "checkbox":
                $("#modalCheckboxesQuestion").modal("show");
                break;
        }
        $("#additem").val("").trigger("chosen:updated");
    }).chosen({ width: "250px", allow_single_deselect: true });
    $('.chosen-search').hide();

    $("#testForm").submit(function () {
        $("#questions").val(JSON.stringify(questions));
    });

    CKEDITOR.replace("options", {
        extraPlugins: "gapscreator",
        removePlugins: "filebrowser,a11yhelp,bidi,blockquote,panelbutton,colorbutton,colordialog,templates,link,smiley,print,div",
    });

    $(".add-option").click(function () {
        if (currentType) {
            var opt;
            switch (currentType) {
                case "radio":
                    opt = $(".base-option-radio").clone(true, true).removeClass("base-option-radio").show();
                    break;
                case "checkbox":
                    opt = $(".base-option-checkbox").clone(true, true).removeClass("base-option-checkbox").show();
                    break;
            }
        }
        if (opt)
            $(this).before(opt);
    });

    $(".delete-option").click(function () {
        if ($(".option:not(.base-option-radio):not(.base-option-checkbox)")
            && $(".option:not(.base-option-radio):not(.base-option-checkbox)").length > 1)
            $(this).parent().remove();
    });

    //Event onclick for edit button
    $(".edit-question").click(function () {
        EditQuestion($(this).parent().parent().find("input[type=hidden]").val());
    });

    $(".delete-question").click(function () {
        DeleteQuestion($(this).parent().parent().find("input[type=hidden]").val());
        $(this).parent().parent().remove();
    });
});

function escapeSpecialChars(jsonString) {

    return jsonString.replace(/\n/g, "\\n")
        .replace(/\r/g, "\\r")
        .replace(/\t/g, "\\t")
        .replace(/\f/g, "\\f");

}

function SaveQuestion(modal) {
    var form = modal.find("form");
    if (!form.valid())
        return;
    var type = modal.find("#type").val();
    var ID = modal.find("#ID").val();
    if (!ID)
        ID = GetQuestionID();
    var text = modal.find("#Text").val();
    var helpText = modal.find("#HelpText").val();
    var question = {ID: ID, Text: text, HelpText: helpText, Type: type};
    switch (type) {
        case "text":
            var textWithGaps = CKEDITOR.instances.options.getData();
            question.Options = [{ Text: textWithGaps, IsRight: true }];
            break;
        case "radio":
            question.Options = [];
            modal.find(".option").each(function (index) {
                var text = $(this).find("input[type=text]").val();
                var isRight = $(this).find("input[type=radio]").prop("checked");
                var option = { Text: text, IsRight: isRight };
                if (text)
                    question.Options.push(option);
            });
            break;
        case "checkbox":
            question.Options = [];
            modal.find(".option").each(function (index) {
                var text = $(this).find("input[type=text]").val();
                var isRight = $(this).find("input[type=checkbox]").prop("checked");
                var option = { Text: text, IsRight: isRight };
                if (text)
                    question.Options.push(option);
            });
            break;
    }
    SaveQuestionInVar(question);
    if (currentQuestion != ID) {
        var questionDiv = $(".base-question").clone(true, true).removeClass("base-question").show();
        questionDiv.find("input[type=hidden]").val(question.ID);
        questionDiv.find("label").append(question.Text);
        $(".questions").append(questionDiv);
    }
    else {
        $(".question").each(function (index, element) {
            if ($(element).find("input[type=hidden]").val() == currentQuestion) {
                $(element).find("label").text(question.Text);
            }
        });
    }
    modal.modal("hide");
}

function SaveQuestionInVar(question) {
    var index;
    if (questions) {
        for (var i = 0; i < questions.length; i++) {
            var q = questions[i];
            if (q && q.ID && q.ID == question.ID)
                index = i;
        }
    }
    if (index >= 0) {
        questions.splice(index, 1);
        questions[i] = question;
    }
    else {
        questions[questions.length] = question;
    }
}

function EditQuestion(id) {
    currentQuestion = id;
    var question = GetQuestion(id);
    if (question) {
        switch (question.Type) {
            case "text":
            case 1:
                $("#modalTextQuestion").modal("show");
                break;
            case "radio":
            case 3:
                $("#modalMultipleChoiceQuestion").modal("show");
                break;
            case "checkbox":
            case 2:
                $("#modalCheckboxesQuestion").modal("show");
                break;
        }
    }
}

function DeleteQuestion(id) {
    if (questions && questions.length != 0) {
        for (var i = 0; i < questions.length; i++) {
            var question = questions[i];
            if (question && question.ID && question.ID == id) {
                questions.splice(i, 1);
                break;
            }
        }
    }
}
//Get question by id
function GetQuestion(id) {
    if (questions && questions.length != 0) {
        for (var i = 0; i < questions.length; i++) {
            var question = questions[i];
            if (question && question.ID && question.ID == id) {
                return question;
            }
        }
    }
    return null;
}

function GetQuestionID() {
    var maxID = 1;
    if (questions) {
        for (var i = 0; i < questions.length; i++) {
            var question = questions[i];
            if (question && question.ID) {
                if (question.ID >= maxID)
                    maxID = question.ID + 1;
            }
        }
    }
    return maxID;
}

$(".modal").on("show.bs.modal", function (e) {
    var modal = $(this);
    var type = modal.find("#type").val();
    currentType = type;
    var optionsDiv = modal.find(".options");
    if (currentQuestion && optionsDiv && type) {
        var question = GetQuestion(currentQuestion);
        if (question) {
            modal.find("#ID").val(question.ID);
            modal.find("#Text").val(question.Text);
            modal.find("#HelpText").val(question.HelpText);
            switch (type) {
                case "text":
                    if(question.Options && question.Options[0])
                        CKEDITOR.instances.options.editable().setHtml(question.Options[0].Text);
                    break;
                case "radio":
                    if (question.Options && question.Options.length != 0) {
                        var opts = [];
                        for (var i = 0; i < question.Options.length; i++) {
                            var option = question.Options[i];
                            var opt;
                            opt = $(".base-option-radio").clone(true, true).removeClass("base-option-radio").show();
                            opt.find("input[type=radio]").prop('checked', option.IsRight);
                            opt.find("input[type=text]").val(option.Text);
                            opts.push(opt);
                        }
                        if (opts && opts.length != 0) {
                            optionsDiv.prepend(opts);
                        }
                    }
                    else {
                        var opt;
                        opt = $(".base-option-radio").clone(true, true).removeClass("base-option-radio").show();
                        opt.find("input[type=radio]").prop('checked', true);
                        if (opt)
                            optionsDiv.prepend(opt);
                    }
                    break;
                case "checkbox":
                    if (question.Options && question.Options.length != 0) {
                        var opts = [];
                        for (var i = 0; i < question.Options.length; i++) {
                            var option = question.Options[i];
                            var opt;
                            opt = $(".base-option-checkbox").clone(true, true).removeClass("base-option-checkbox").show();
                            opt.find("input[type=checkbox]").prop('checked', option.IsRight);
                            opt.find("input[type=text]").val(option.Text);
                            opts.push(opt);
                        }
                        if (opts && opts.length != 0) {
                            optionsDiv.prepend(opts);
                        }
                    }
                    else {
                        var opt;
                        opt = $(".base-option-checkbox").clone(true, true).removeClass("base-option-checkbox").show();
                        if (opt)
                            optionsDiv.prepend(opt);
                    }
                    break;
            }
        }
    }
    else if (optionsDiv && type) {
        var opt;
        switch (type) {
            case "radio":
                opt = $(".base-option-radio").clone(true, true).removeClass("base-option-radio").show();
                opt.find("input[type=radio]").prop('checked', true);
                break;
            case "checkbox":
                opt = $(".base-option-checkbox").clone(true, true).removeClass("base-option-checkbox").show();
                break;
        }
        if (opt)
            optionsDiv.prepend(opt);
    }
});

$(".modal").on("hidden.bs.modal", function (e) {
    currentType = "";
    currentQuestion = "";
    CKEDITOR.instances.options.setData("");
    var modal = $(this);
    modal.find("#Text").val("");
    modal.find("#HelpText").val("");
    modal.find(".option").remove();
});

$.fn.modal.Constructor.prototype.enforceFocus = function () {
    modal_this = this
    $(document).on('focusin.modal', function (e) {
        if (modal_this.$element[0] !== e.target && !modal_this.$element.has(e.target).length
        && !$(e.target.parentNode).hasClass("cke_dialog_ui_input_select")
        && !$(e.target.parentNode).hasClass("cke_dialog_ui_input_text")) {
            modal_this.$element.focus()
        }
    })
};